//
// RemotePowerShellHost.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.DotNetCore;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.PackageManagement.Protocol;
using MonoDevelop.Projects;
using NuGet.Common;
using NuGet.PackageManagement.VisualStudio;
using NuGetConsole;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.Scripting
{
	class RemotePowerShellHost : IPowerShellHost
	{
		Process process;
		JsonRpc rpc;
		PowerShellHostMessageHandler messageHandler;
		ItemOperationsMessageHandler itemOperationsMessageHandler;
		SolutionMessageHandler solutionMessageHandler;
		ProjectMessageHandler projectMessageHandler;
		List<string> modulesToImport = new List<string> ();
		IScriptingConsole scriptingConsole;
		DateTime? restartProcessTime;

		public RemotePowerShellHost (IScriptingConsole scriptingConsole)
		{
			this.scriptingConsole = scriptingConsole;
		}

		public IList<string> ModulesToImport => modulesToImport;
		public Version Version => NuGetVersion.Version;

		public event EventHandler Exited;

		public void ExecuteCommand (string command)
		{
			try {
				EnsureHostInitialized ();
				rpc.InvokeAsync (Methods.InvokeName, command).Wait ();
			} catch (Exception ex) {
				string errorMessage = ExceptionUtilities.DisplayMessage (ex);
				scriptingConsole.WriteLine (errorMessage, ScriptingStyle.Error);
			}
		}

		void EnsureHostInitialized ()
		{
			if (messageHandler != null)
				return;

			if (!ShouldRestartProcess ()) {
				throw new ApplicationException (GettextCatalog.GetString ("Unable to restart PowerShell host"));
			}

			process = StartPowerShellHost ();
			process.Exited += Process_Exited;

			messageHandler = new PowerShellHostMessageHandler (scriptingConsole);

			rpc = new JsonRpc (process.StandardInput.BaseStream, process.StandardOutput.BaseStream, messageHandler);
			rpc.Disconnected += JsonRpcDisconnected;

			itemOperationsMessageHandler = new ItemOperationsMessageHandler ();
			rpc.AddLocalRpcTarget (itemOperationsMessageHandler);

			solutionMessageHandler = new SolutionMessageHandler ();
			rpc.AddLocalRpcTarget (solutionMessageHandler);

			projectMessageHandler = new ProjectMessageHandler ();
			rpc.AddLocalRpcTarget (projectMessageHandler);

			rpc.StartListening ();
		}

		static Process StartPowerShellHost ()
		{
			var programPath = Path.Combine (Path.GetDirectoryName (typeof (RemotePowerShellHost).Assembly.Location), "PowerShellConsoleHost", "MonoDevelop.PackageManagement.PowerShell.ConsoleHost.dll");

			var argumentBuilder = new ProcessArgumentBuilder ();
			argumentBuilder.AddQuoted (programPath);
			argumentBuilder.AddQuoted (UserProfile.Current.LogDir);

			var info = new ProcessStartInfo {
				FileName = DotNetCoreRuntime.FileName,
				Arguments = argumentBuilder.ToString (),
				WorkingDirectory = Path.GetDirectoryName (programPath),
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true
			};

			var process = new Process {
				StartInfo = info
			};
			process.Start ();

			return process;
		}

		void JsonRpcDisconnected (object sender, JsonRpcDisconnectedEventArgs e)
		{
			string errorMessage = GettextCatalog.GetString (
				"Disconnected from remote PowerShell host. Reason: {0}: {1}",
				e.Reason,
				e.Description);
			scriptingConsole.WriteLine (errorMessage, ScriptingStyle.Error);

			process.Exited -= Process_Exited;
			rpc.Disconnected -= JsonRpcDisconnected;
			messageHandler = null;

			Exited?.Invoke (this, EventArgs.Empty);
		}

		static void Process_Exited (object sender, EventArgs e)
		{
			LoggingService.LogInfo ("PowerShell host exited");
		}

		public void OnActiveSourceChanged (SourceRepositoryViewModel source)
		{
			if (rpc == null)
				return;

			try {
				var message = new ActivePackageSourceChangedParams {
					ActiveSource = GetPackageSource (source)
				};
				rpc.InvokeAsync (Methods.ActiveSourceName, message).Ignore ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnActiveSourceChanged error", ex);
			}
		}

		public void OnPackageSourcesChanged (IEnumerable<SourceRepositoryViewModel> sources, SourceRepositoryViewModel selectedPackageSource)
		{
			try {
				EnsureHostInitialized ();
				var message = new PackageSourcesChangedParams {
					ActiveSource = GetPackageSource (selectedPackageSource),
					Sources = GetPackageSources (sources).ToArray ()
				};
				rpc.InvokeAsync (Methods.PackageSourcesChangedName, message).Ignore ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnPackageSourcesChanged error", ex);
			}
		}

		IEnumerable<PackageSource> GetPackageSources (IEnumerable<SourceRepositoryViewModel> sources)
		{
			return sources.Select (source => GetPackageSource (source));
		}

		PackageSource GetPackageSource (SourceRepositoryViewModel sourceRepositoryViewModel)
		{
			if (sourceRepositoryViewModel == null)
				return null;

			return new PackageSource {
				Name = sourceRepositoryViewModel.Name,
				Source = sourceRepositoryViewModel.PackageSource.Source,
				IsAggregate = sourceRepositoryViewModel.IsAggregate
			};
		}

		public void OnMaxVisibleColumnsChanged (int columns)
		{
			if (rpc == null) {
				// Column information should be updated after the first PowerShell command is run with
				// remote PowerShell host.
				return;
			}

			try {
				rpc.InvokeAsync (Methods.MaxVisibleColumnsChangedName, columns).Ignore ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnMaxVisibleColumnsChanged error", ex);
			}
		}

		public void SolutionLoaded (Solution solution)
		{
			if (rpc == null)
				return;

			try {
				var message = new SolutionParams {
					FileName = solution.FileName
				};
				rpc.InvokeAsync (Methods.SolutionLoadedName, message).Ignore ();
			} catch (Exception ex) {
				LoggingService.LogError ("SolutionLoaded error", ex);
			}
		}

		public void SolutionUnloaded ()
		{
			if (rpc == null)
				return;

			try {
				rpc.InvokeAsync (Methods.SolutionUnloadedName).Ignore ();
			} catch (Exception ex) {
				LoggingService.LogError ("SolutionUnloaded error", ex);
			}
		}

		public void OnDefaultProjectChanged (Project project)
		{
			if (rpc == null)
				return;

			try {
				var message = new DefaultProjectChangedParams {
					FileName = project?.FileName
				};
				rpc.InvokeAsync (Methods.DefaultProjectChangedName, message).Ignore ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnDefaultProjectChanged error", ex);
			}
		}

		public void StopCommand ()
		{
			if (rpc == null)
				return;

			try {
				rpc.NotifyAsync (Methods.StopCommandName).Ignore ();
			} catch (Exception ex) {
				LoggingService.LogError ("StopCommand error", ex);
			}
		}

		public IScriptExecutor CreateScriptExecutor ()
		{
			EnsureHostInitialized ();
			return new ScriptExecutor (rpc);
		}

		public ITabExpansion CreateTabExpansion ()
		{
			return new TabExpansion (rpc);
		}

		bool ShouldRestartProcess ()
		{
			if (process == null) {
				// First attempt.
				return true;
			}

			if (restartProcessTime == null) {
				restartProcessTime = DateTime.UtcNow;
				return true;
			}

			var restartAllowedInterval = TimeSpan.FromSeconds (5);
			var currentTime = DateTime.UtcNow;
			if (currentTime > restartProcessTime + restartAllowedInterval) {
				return false;
			}

			return true;
		}
	}
}

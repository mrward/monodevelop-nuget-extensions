//
// PowerShellConsoleHost.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2022 Microsoft
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.DotNetCore;
using MonoDevelop.PackageManagement.PowerShell;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;
using NuGet.Common;
using NuGet.PackageManagement.VisualStudio;
using NuGetConsole;

namespace MonoDevelop.PackageManagement.Scripting
{
	class PowerShellConsoleHost : IPowerShellHost, ITabExpansion
	{
		readonly IScriptingConsole scriptingConsole;
		readonly object dte;
		readonly object privateData;

		List<string> modulesToImport = new List<string> ();

		Runspace runspace;
		PowerShellHost host;

		Pipeline currentPipeline;
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource ();

		public PowerShellConsoleHost (
			IScriptingConsole scriptingConsole,
			Version version,
			object privateData,
			object dte)
		{
			this.scriptingConsole = scriptingConsole;
			this.privateData = privateData;
			this.dte = dte;
			Version = version;
		}

		public IList<string> ModulesToImport => modulesToImport;
		public Version Version { get; }

		public event EventHandler Exited;

		public void ExecuteCommand (string command)
		{
			try {
				EnsureHostInitialized ();
				InvokePowerShellInternal (command);
			} catch (Exception ex) {
				string errorMessage = ExceptionUtilities.DisplayMessage (ex);
				scriptingConsole.WriteLine (errorMessage, ScriptingStyle.Error);
			}
		}

		void EnsureHostInitialized ()
		{
			if (host != null)
				return;

			ConfigurePathEnvironmentVariable ();
			CreatePowerShellHost ();
		}

		void CreatePowerShellHost ()
		{
			host = new PowerShellHost (scriptingConsole, Version, privateData);

			var initialSessionState = CreateInitialSessionState ();
			runspace = RunspaceFactory.CreateRunspace (host, initialSessionState);
			runspace.Open ();
		}

		InitialSessionState CreateInitialSessionState ()
		{
			var initialSessionState = InitialSessionState.CreateDefault ();
			string[] modulesToImport = PowerShellModules.GetModules ().ToArray ();
			initialSessionState.ImportPSModule (modulesToImport);
			SessionStateVariableEntry variable = CreateDTESessionVariable ();
			initialSessionState.Variables.Add (variable);
			return initialSessionState;
		}

		SessionStateVariableEntry CreateDTESessionVariable ()
		{
			var options = ScopedItemOptions.AllScope | ScopedItemOptions.Constant;
			return new SessionStateVariableEntry ("DTE", dte, "DTE object", options);
		}

		/// <summary>
		/// Ensure dotnet is on the PATH.
		/// </summary>
		static void ConfigurePathEnvironmentVariable ()
		{
			if (DotNetCoreRuntime.IsMissing) {
				return;
			}

			string dotNetDirectory = Path.GetDirectoryName (DotNetCoreRuntime.FileName);
			if (string.IsNullOrEmpty (dotNetDirectory)) {
				return;
			}

			string path = Environment.GetEnvironmentVariable ("PATH");
			if (!string.IsNullOrEmpty (path)) {
				path += Path.PathSeparator + dotNetDirectory;
				Environment.SetEnvironmentVariable ("PATH", path);
			}
		}

		public void OnActiveSourceChanged (SourceRepositoryViewModel source)
		{
			//if (rpc == null)
			//	return;

			//try {
			//	var message = new ActivePackageSourceChangedParams {
			//		ActiveSource = GetPackageSource (source)
			//	};
			//	rpc.InvokeAsync (Methods.ActiveSourceName, message).Ignore ();
			//} catch (Exception ex) {
			//	LoggingService.LogError ("OnActiveSourceChanged error", ex);
			//}
		}

		public void OnPackageSourcesChanged (IEnumerable<SourceRepositoryViewModel> sources, SourceRepositoryViewModel selectedPackageSource)
		{
			//try {
			//	EnsureHostInitialized ();
			//	var message = new PackageSourcesChangedParams {
			//		ActiveSource = GetPackageSource (selectedPackageSource),
			//		Sources = GetPackageSources (sources).ToArray ()
			//	};
			//	rpc.InvokeAsync (Methods.PackageSourcesChangedName, message).Ignore ();
			//} catch (Exception ex) {
			//	LoggingService.LogError ("OnPackageSourcesChanged error", ex);
			//}
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
			//if (rpc == null) {
			//	// Column information should be updated after the first PowerShell command is run with
			//	// remote PowerShell host.
			//	return;
			//}

			//try {
			//	rpc.InvokeAsync (Methods.MaxVisibleColumnsChangedName, columns).Ignore ();
			//} catch (Exception ex) {
			//	LoggingService.LogError ("OnMaxVisibleColumnsChanged error", ex);
			//}
		}

		public void SolutionLoaded (Solution solution)
		{
			//if (rpc == null)
			//	return;

			//try {
			//	var message = new SolutionParams {
			//		FileName = solution.FileName
			//	};
			//	rpc.InvokeAsync (Methods.SolutionLoadedName, message).Ignore ();
			//} catch (Exception ex) {
			//	LoggingService.LogError ("SolutionLoaded error", ex);
			//}
		}

		public void SolutionUnloaded ()
		{
			//if (rpc == null)
			//	return;

			//try {
			//	rpc.InvokeAsync (Methods.SolutionUnloadedName).Ignore ();
			//} catch (Exception ex) {
			//	LoggingService.LogError ("SolutionUnloaded error", ex);
			//}
		}

		public void OnDefaultProjectChanged (Project project)
		{
			//if (rpc == null)
			//	return;

			//try {
			//	var message = new DefaultProjectChangedParams {
			//		FileName = project?.FileName
			//	};
			//	rpc.InvokeAsync (Methods.DefaultProjectChangedName, message).Ignore ();
			//} catch (Exception ex) {
			//	LoggingService.LogError ("OnDefaultProjectChanged error", ex);
			//}
		}

		public void StopCommand ()
		{
			//if (rpc == null)
			//	return;

			//try {
			//	rpc.NotifyAsync (Methods.StopCommandName).Ignore ();
			//} catch (Exception ex) {
			//	LoggingService.LogError ("StopCommand error", ex);
			//}
		}

		public IScriptExecutor CreateScriptExecutor ()
		{
			EnsureHostInitialized ();
			return new ScriptExecutor ();
		}

		public ITabExpansion CreateTabExpansion ()
		{
			return this;
		}

		void InvokePowerShellInternal (string line, params object[] input)
		{
			try {
				RefreshHostCancellationToken ();
				using (var pipeline = CreatePipeline (runspace, line)) {
					currentPipeline = pipeline;
					pipeline.Invoke (input);
					CheckPipelineState (pipeline);
				}
			} finally {
				currentPipeline = null;
			}
		}

		void RefreshHostCancellationToken ()
		{
			if (cancellationTokenSource.IsCancellationRequested) {
				cancellationTokenSource.Dispose ();
				cancellationTokenSource = new CancellationTokenSource ();
			}

			host.SetPropertyValueOnHost ("CancellationTokenKey", cancellationTokenSource.Token);
		}

		void RefreshHostCancellationToken (CancellationToken token)
		{
			cancellationTokenSource.Dispose ();
			cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource (token);

			host.SetPropertyValueOnHost ("CancellationTokenKey", cancellationTokenSource.Token);
		}

		static Pipeline CreatePipeline (Runspace runspace, string command)
		{
			Pipeline pipeline = runspace.CreatePipeline ();
			pipeline.Commands.AddScript (command, false);
			//pipeline.Commands.Add ("out-default");
			pipeline.Commands.Add ("out-host"); // Ensures native command output goes through the HostUI.
			pipeline.Commands[0].MergeMyResults (PipelineResultTypes.Error, PipelineResultTypes.Output);
			return pipeline;
		}

		void CheckPipelineState (Pipeline pipeline)
		{
			switch (pipeline.PipelineStateInfo?.State) {
				case PipelineState.Completed:
				case PipelineState.Stopped:
				case PipelineState.Failed:
					if (pipeline.PipelineStateInfo.Reason != null) {
						ReportError (pipeline.PipelineStateInfo.Reason);
					}
					break;
			}
		}

		void ReportError (Exception exception)
		{
			exception = ExceptionUtilities.Unwrap (exception);
			scriptingConsole.WriteLine (exception.Message, ScriptingStyle.Error);
		}

		public async Task<string[]> GetExpansionsAsync (
			string line,
			string lastWord,
			CancellationToken token)
		{
			try {
				string[] expansions = await Task.Run (() => {
					return RunTabExpansionInternal (line, lastWord, token);
				}, token);
				return expansions;
			} catch (OperationCanceledException) {
				// Ignore
			} catch (Exception ex) {
				LoggingService.LogError (string.Format ("Error getting tab expansions {0}", ex));
			}

			return Array.Empty<string> ();
		}

		string[] RunTabExpansionInternal (string line, string lastWord, CancellationToken token)
		{
			string script = @"$__pc_args=@();$input|%{$__pc_args+=$_};if(Test-Path Function:\TabExpansion2){(TabExpansion2 $__pc_args[0] $__pc_args[0].length).CompletionMatches|%{$_.CompletionText}}else{TabExpansion $__pc_args[0] $__pc_args[1]};Remove-Variable __pc_args -Scope 0;";
			var input = new object[] { line, lastWord };

			Collection<PSObject> results = InvokePowerShellNoOutput (script, input, token);

			if (results != null) {
				return results.Select (item => item?.ToString ())
					.ToArray ();
			}

			return Array.Empty<string> ();
		}

		Collection<PSObject> InvokePowerShellNoOutput (string line, object[] input, CancellationToken token)
		{
			try {
				RefreshHostCancellationToken (token);
				using (Pipeline pipeline = runspace.CreatePipeline ()) {
					pipeline.Commands.AddScript (line, false);
					currentPipeline = pipeline;
					return pipeline.Invoke (input);
				}
			} finally {
				currentPipeline = null;
			}
			return null;
		}
	}
}


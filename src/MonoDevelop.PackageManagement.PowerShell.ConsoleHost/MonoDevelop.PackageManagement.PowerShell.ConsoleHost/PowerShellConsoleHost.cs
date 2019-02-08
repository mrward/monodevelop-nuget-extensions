//
// PowerShellConsoleHost.cs
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
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using Newtonsoft.Json.Linq;
using NuGet.PackageManagement.PowerShellCmdlets;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging.Core;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost
{
	class PowerShellConsoleHost
	{
		static PowerShellConsoleHost instance;

		JsonRpc rpc;
		Runspace runspace;
		PowerShellHost host;
		DTE dte;
		Pipeline currentPipeline;
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource ();

		public static PowerShellConsoleHost Instance => instance;

		public PowerShellConsoleHost ()
		{
			instance = this;
		}

		public void Run ()
		{
			Logger.Log ("PowerShellConsoleHost starting...");

			Stream sender = Console.OpenStandardOutput ();
			Stream reader = Console.OpenStandardInput ();

			host = new PowerShellHost ();
			dte = new DTE ();
			ConsoleHostServices.Initialize (dte);

			var initialSessionState = CreateInitialSessionState ();
			runspace = RunspaceFactory.CreateRunspace (host, initialSessionState);
			runspace.Open ();

			rpc = JsonRpc.Attach (sender, reader, this);
			rpc.Disconnected += OnRpcDisconnected;

			JsonRpcProvider.Rpc = rpc;

			Logger.Log ("PowerShellConsoleHost running...");

			rpc.Completion.Wait ();
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

		void OnRpcDisconnected (object sender, JsonRpcDisconnectedEventArgs e)
		{
			Logger.Log ("PowerShellConsoleHost disconnected: {0}", e.Description);
		}

		[JsonRpcMethod (Methods.InvokeName)]
		public void InvokePowerShell (string line)
		{
			Logger.Log ("PowerShellConsoleHost.Invoke: {0}", line);
			InvokePowerShellInternal (line);
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
			} catch (Exception ex) {
				string errorMessage = NuGet.Common.ExceptionUtilities.DisplayMessage (ex);
				Log (LogLevel.Error, errorMessage);
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
			exception = NuGet.Common.ExceptionUtilities.Unwrap (exception);
			Log (LogLevel.Error, exception.Message);
		}

		internal void Log (LogLevel level, string message)
		{
			var logMessage = new LogMessageParams {
				Level = level,
				Message = message
			};
			rpc.InvokeAsync (Methods.LogName, logMessage).Wait ();
		}

		[JsonRpcMethod (Methods.ActiveSourceName)]
		public void OnActiveSourceChanged (JToken arg)
		{
			Logger.Log ("PowerShellConsoleHost.ActiveSourceChanged");
			try {
				var message = arg.ToObject<ActivePackageSourceChangedParams> ();
				Logger.Log ("PowerShellConsoleHost.ActiveSourceChanged {0}", message.ActiveSource);

				host.SetPropertyValueOnHost ("activePackageSource", GetActivePackageSourceName (message.ActiveSource));
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error changing active source. {0}", ex));
			}
		}

		[JsonRpcMethod (Methods.PackageSourcesChangedName)]
		public void OnPackageSourcesChanged (JToken arg)
		{
			Logger.Log ("PowerShellConsoleHost.OnPackageSourcesChanged");
			try {
				var message = arg.ToObject<PackageSourcesChangedParams> ();

				Logger.Log ("PowerShellConsoleHost.OnPackageSourcesChanged ActiveSource: {0}", message.ActiveSource?.Name);
				foreach (var source in message.Sources) {
					Logger.Log ("PowerShellConsoleHost.OnPackageSourcesChanged Source: {0}", source.Name);
				}

				ConsoleHostServices.SourceRepositoryProvider.UpdatePackageSources (message.Sources);
				host.SetPropertyValueOnHost ("activePackageSource", GetActivePackageSourceName (message.ActiveSource));
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error updating package sources. {0}", ex));
			}
		}

		static string GetActivePackageSourceName (PackageSource source)
		{
			if (source == null)
				return null;

			if (source.IsAggregate) {
				// Cmdlets will use all enabled sources if the active package source is not defined.
				return null;
			}

			return source.Name;
		}

		[JsonRpcMethod (Methods.MaxVisibleColumnsChangedName)]
		public void OnMaxVisibleColumnsChanged (int maxVisibleColumns)
		{
			Logger.Log ("PowerShellConsoleHost.OnMaxVisibleColumnsChanged: {0}", maxVisibleColumns);
			try {
				host.MaxVisibleColumns = maxVisibleColumns;
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error updating package sources. {0}", ex));
			}
		}

		[JsonRpcMethod (Methods.SolutionLoadedName)]
		public void OnSolutionLoaded (JToken arg)
		{
			Logger.Log ("PowerShellConsoleHost.OnSolutionLoaded");
			try {
				var message = arg.ToObject<SolutionParams> ();
				Logger.Log ("PowerShellConsoleHost.OnSolutionLoaded: {0}", message.FileName);

				ConsoleHostServices.SolutionManager.OnSolutionLoaded (message.FileName);
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error on solution loaded. {0}", ex));
			}
		}

		[JsonRpcMethod (Methods.SolutionUnloadedName)]
		public void OnSolutionUnloaded ()
		{
			Logger.Log ("PowerShellConsoleHost.OnSolutionUnloaded");
			try {
				ConsoleHostServices.SolutionManager.OnSolutionUnloaded ();
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error on solution loaded. {0}", ex));
			}
		}

		[JsonRpcMethod (Methods.DefaultProjectChangedName)]
		public void OnDefaultProjectChanged (JToken arg)
		{
			Logger.Log ("PowerShellConsoleHost.OnDefaultProjectChanged");
			try {
				var message = arg.ToObject<DefaultProjectChangedParams> ();
				Logger.Log ("PowerShellConsoleHost.OnDefaultProjectChanged {0}", message.FileName);

				ConsoleHostServices.SolutionManager.DefaultProjectFileName = message.FileName;
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error changing active source. {0}", ex));
			}
		}

		[JsonRpcMethod (Methods.StopCommandName)]
		public void OnStopCommand ()
		{
			Logger.Log ("PowerShellConsoleHost.OnStopCommand");
			try {
				cancellationTokenSource.Cancel ();
				Pipeline pipeline = currentPipeline;
				pipeline?.StopAsync ();
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error stopping command {0}", ex));
			}
		}

		[JsonRpcMethod (Methods.RunScript)]
		public RunScriptResult RunScript (JToken arg)
		{
			Logger.Log ("PowerShellConsoleHost.RunScript");
			try {
				var message = arg.ToObject<RunScriptParams> ();

				RunScriptInternal (message);

				return new RunScriptResult {
					Success = true
				};
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error running script {0}", ex));
				return new RunScriptResult {
					ErrorMessage = ex.Message
				};
			}
		}

		void RunScriptInternal (RunScriptParams message)
		{
			var blockingCollection = ConsoleHostServices.ActiveBlockingCollection;

			var version = NuGet.Versioning.NuGetVersion.Parse (message.PackageVersion);
			var identity = new PackageIdentity (message.PackageId, version);
			var project = new Project (message.Project);

			var scriptMessage = new ScriptMessage (
				message.ScriptPath,
				message.InstallPath,
				identity,
				project);

			blockingCollection.Add (scriptMessage);

			WaitHandle.WaitAny (new WaitHandle[] { scriptMessage.EndSemaphore, cancellationTokenSource.Token.WaitHandle });

			if (scriptMessage.Exception == null) {
				return;
			}

			if (message.ThrowOnFailure) {
				throw scriptMessage.Exception;
			}

			Log (LogLevel.Warning, scriptMessage.Exception.Message);
		}

		[JsonRpcMethod (Methods.RunInitScript)]
		public RunScriptResult RunInitScript (JToken arg)
		{
			Logger.Log ("PowerShellConsoleHost.RunInitScript");
			try {
				var message = arg.ToObject<RunInitScriptParams> ();

				RunScriptInternal (message);

				return new RunScriptResult {
					Success = true
				};
			} catch (Exception ex) {
				Logger.Log (string.Format ("Error running script {0}", ex));
				return new RunScriptResult {
					ErrorMessage = ex.Message
				};
			}
		}

		void RunScriptInternal (RunInitScriptParams message)
		{
			var version = NuGet.Versioning.NuGetVersion.Parse (message.PackageVersion);
			var identity = new PackageIdentity (message.PackageId, version);

			var request = new ScriptExecutionRequest (
				message.ScriptPath,
				message.InstallPath,
				identity,
				null);

			try {
				InvokePowerShellInternal (
					request.BuildCommand (),
					request.BuildInput ()
				);
			} catch (Exception ex) {
				if (message.ThrowOnFailure) {
					throw;
				}

				Log (LogLevel.Warning, ex.Message);
			}
		}
	}
}

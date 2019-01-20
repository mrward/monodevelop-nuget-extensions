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
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using Newtonsoft.Json.Linq;
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
			try {
				using (var pipeline = CreatePipeline (runspace, line)) {
					pipeline.Invoke ();
				}
			} catch (Exception ex) {
				string errorMessage = NuGet.Common.ExceptionUtilities.DisplayMessage (ex);
				Log (LogLevel.Error, errorMessage);
			}
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
	}
}

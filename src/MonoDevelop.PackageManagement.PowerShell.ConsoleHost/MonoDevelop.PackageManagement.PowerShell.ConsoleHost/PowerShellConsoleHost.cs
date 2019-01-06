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
using System.Management.Automation.Runspaces;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost
{
	class PowerShellConsoleHost
	{
		static PowerShellConsoleHost instance;

		JsonRpc rpc;
		Runspace runspace;
		PowerShellHost host;

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

			var initialSessionState = CreateInitialSessionState ();
			runspace = RunspaceFactory.CreateRunspace (host, initialSessionState);
			runspace.Open ();

			rpc = JsonRpc.Attach (sender, reader, this);
			rpc.Disconnected += OnRpcDisconnected;

			Logger.Log ("PowerShellConsoleHost running...");

			rpc.Completion.Wait ();
		}

		InitialSessionState CreateInitialSessionState ()
		{
			var initialSessionState = InitialSessionState.CreateDefault ();
			string[] modulesToImport = PowerShellModules.GetModules ().ToArray ();
			initialSessionState.ImportPSModule (modulesToImport);
			return initialSessionState;
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
				Logger.Log ("PowerShellConsoleHost.Invoke error: {0}", ex);
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
			rpc.NotifyWithParameterObjectAsync (Methods.LogName, logMessage).Ignore ();
		}
	}
}

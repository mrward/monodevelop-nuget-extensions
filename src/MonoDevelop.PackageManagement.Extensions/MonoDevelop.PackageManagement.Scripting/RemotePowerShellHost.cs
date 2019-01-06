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
using ICSharpCode.PackageManagement.Scripting;
using ICSharpCode.Scripting;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.PackageManagement.EnvDTE;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using Newtonsoft.Json;
using NuGet.Common;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.Scripting
{
	class RemotePowerShellHost : IPowerShellHost
	{
		Process process;
		JsonRpc rpc;
		PowerShellHostMessageHandler messageHandler;
		ItemOperationsMessageHandler itemOperationsMessageHandler;
		List<string> modulesToImport = new List<string> ();
		IScriptingConsole scriptingConsole;

		public RemotePowerShellHost (IScriptingConsole scriptingConsole)
		{
			this.scriptingConsole = scriptingConsole;
		}

		public IList<string> ModulesToImport => modulesToImport;
		public Version Version => new Version ("1.0");

		public void ExecuteCommand (string command)
		{
			EnsureHostInitialized ();

			try {
				rpc.InvokeAsync (Methods.InvokeName, command).Wait ();
			} catch (Exception ex) {
				string errorMessage = ExceptionUtilities.DisplayMessage (ex);
				scriptingConsole.WriteLine (errorMessage, ScriptingStyle.Error);
			}
		}

		public void SetDefaultRunspace ()
		{
		}

		public void SetRemoteSignedExecutionPolicy ()
		{
		}

		public void UpdateFormatting (IEnumerable<string> formattingFiles)
		{
		}

		void EnsureHostInitialized ()
		{
			if (messageHandler != null)
				return;

			process = StartPowerShellHost ();
			process.Exited += Process_Exited;

			messageHandler = new PowerShellHostMessageHandler (scriptingConsole);
			itemOperationsMessageHandler = new ItemOperationsMessageHandler ();
			rpc = new JsonRpc (process.StandardInput.BaseStream, process.StandardOutput.BaseStream, messageHandler);
			rpc.Disconnected += JsonRpcDisconnected;
			rpc.AddLocalRpcTarget (itemOperationsMessageHandler);
			rpc.StartListening ();
			rpc.JsonSerializer.NullValueHandling = NullValueHandling.Ignore;
		}

		static Process StartPowerShellHost ()
		{
			var programPath = Path.Combine (Path.GetDirectoryName (typeof (RemotePowerShellHost).Assembly.Location), "PowerShellConsoleHost", "MonoDevelop.PackageManagement.PowerShell.ConsoleHost.dll");

			var argumentBuilder = new ProcessArgumentBuilder ();
			argumentBuilder.AddQuoted (programPath);
			argumentBuilder.AddQuoted (UserProfile.Current.LogDir);

			var info = new ProcessStartInfo {
				FileName = "dotnet",
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

		static void JsonRpcDisconnected (object sender, JsonRpcDisconnectedEventArgs e)
		{
			LoggingService.LogInfo ("Disconnected {0} {1}", e.Reason, e.Description);
		}

		static void Process_Exited (object sender, EventArgs e)
		{
			LoggingService.LogInfo ("PowerShell host exited");
		}
	}
}

//
// PowerShellHostMessageHandler.cs
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
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.PackageManagement.Scripting;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using ProtocolLogLevel = MonoDevelop.PackageManagement.PowerShell.Protocol.LogLevel;

namespace MonoDevelop.PackageManagement.Protocol
{
	class PowerShellHostMessageHandler
	{
		readonly IScriptingConsole scriptingConsole;

		public PowerShellHostMessageHandler (IScriptingConsole scriptingConsole)
		{
			this.scriptingConsole = scriptingConsole;
		}

		[JsonRpcMethod (Methods.LogName)]
		public void OnLogMessage (JToken arg)
		{
			try {
				var logMessage = arg.ToObject<LogMessageParams> ();
				switch (logMessage.Level) {
					case ProtocolLogLevel.Error:
						scriptingConsole.WriteLine (logMessage.Message, ScriptingStyle.Error);
						break;
					case ProtocolLogLevel.Warning:
						scriptingConsole.WriteLine (logMessage.Message, ScriptingStyle.Warning);
						break;
					case ProtocolLogLevel.Verbose:
						scriptingConsole.WriteLine (logMessage.Message, ScriptingStyle.Out);
						break;
					case ProtocolLogLevel.Debug:
						scriptingConsole.WriteLine (logMessage.Message, ScriptingStyle.Debug);
						break;
					default:
						scriptingConsole.WriteLine (logMessage.Message, ScriptingStyle.Out);
						break;
				}
			} catch (Exception ex) {
				LoggingService.LogError ("OnLogMessage error: {0}", ex);
			}
		}

		[JsonRpcMethod (Methods.ClearHostName)]
		public void OnClearHost ()
		{
			scriptingConsole.Clear ();
		}

		[JsonRpcMethod (Methods.ShowConsoleName)]
		public void OnShowConsole ()
		{
			Runtime.RunInMainThread (() => {
				var pad = IdeApp.Workbench.GetPad <PackageConsolePad> ();
				pad.BringToFront ();
			}).Ignore ();
		}

		[JsonRpcMethod (Methods.PromptForInputName)]
		public PromptForInputResponse OnPromptForInput (JToken arg)
		{
			var message = arg.ToObject<PromptForInputParams> ();

			string input = scriptingConsole.PromptForInput (message.Message).WaitAndGetResult ();
			return new PromptForInputResponse {
				Line = input
			};
		}
	}
}

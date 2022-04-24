//
// PowerShellUserInterfaceHost.cs
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
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using MonoDevelop.PackageManagement.Scripting;
using NuGetConsole.Host.PowerShell.Implementation;

namespace MonoDevelop.PackageManagement.PowerShell
{
	class PowerShellUserInterfaceHost : PSHostUserInterface
	{
		IScriptingConsole scriptingConsole;
		StringBuilder messageBuilder = new StringBuilder ();
		PowerShellRawUserInterface rawUI;
		PowerShellUserInterfaceHostPrompt hostPrompt;

		public PowerShellUserInterfaceHost (IScriptingConsole scriptingConsole)
		{
			this.scriptingConsole = scriptingConsole;
			rawUI = new PowerShellRawUserInterface (scriptingConsole);
			hostPrompt = new PowerShellUserInterfaceHostPrompt (scriptingConsole);
		}

		public override PSHostRawUserInterface RawUI => rawUI;

		public int MaxVisibleColumns {
			get { return rawUI.MaxVisibleColumns; }
			set { rawUI.MaxVisibleColumns = value; }
		}

		public override Dictionary<string, PSObject> Prompt (string caption, string message, Collection<FieldDescription> descriptions)
		{
			return null;
		}

		public override int PromptForChoice (string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			return hostPrompt.PromptForChoice (caption, message, choices, defaultChoice);
		}

		public override PSCredential PromptForCredential (string caption, string message, string userName, string targetName)
		{
			return null;
		}

		public override PSCredential PromptForCredential (string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			return null;
		}

		public override string ReadLine ()
		{
			return null;
		}

		public override SecureString ReadLineAsSecureString ()
		{
			return null;
		}

		public override void Write (ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Write (value);
		}

		public override void Write (string value)
		{
			if (value.EndsWith ('\n')) {
				if (messageBuilder.Length > 0) {
					messageBuilder.Append (value.TrimEnd ());
					LogSavedMessageText ();
				} else {
					scriptingConsole.WriteLine (value.TrimEnd (), ScriptingStyle.Out);
				}
			} else {
				messageBuilder.Append (value.TrimEnd ());
			}
		}

		void LogSavedMessageText ()
		{
			if (messageBuilder.Length == 0) {
				return;
			}

			string message = messageBuilder.ToString ();
			messageBuilder.Clear ();

			scriptingConsole.WriteLine (message, ScriptingStyle.Out);
		}

		public override void WriteDebugLine (string message)
		{
			scriptingConsole.WriteLine (message, ScriptingStyle.Debug);
		}

		public override void WriteErrorLine (string value)
		{
			scriptingConsole.WriteLine (value, ScriptingStyle.Error);
		}

		public override void WriteLine (string value)
		{
			scriptingConsole.WriteLine (value, ScriptingStyle.Out);
		}

		public override void WriteProgress (long sourceId, ProgressRecord record)
		{
		}

		public override void WriteVerboseLine (string message)
		{
			scriptingConsole.WriteLine (message, ScriptingStyle.Debug);
		}

		public override void WriteWarningLine (string message)
		{
			scriptingConsole.WriteLine (message, ScriptingStyle.Warning);
		}
	}
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
// Based on parts of NuGetHostUserInterface.cs
// https://github.com/NuGet/NuGet.Client/blob/3803820961f4d61c06d07b179dab1d0439ec0d91/src/NuGet.Clients/NuGetConsole.Host.PowerShell/NuGetHostUserInterface.cs

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using MonoDevelop.PackageManagement.Scripting;

namespace NuGetConsole.Host.PowerShell.Implementation
{
	class PowerShellUserInterfaceHostPrompt
	{
		readonly IScriptingConsole scriptingConsole;

		public PowerShellUserInterfaceHostPrompt (IScriptingConsole scriptingConsole)
		{
			this.scriptingConsole = scriptingConsole;
		}

		internal int PromptForChoice (
			string caption,
			string message,
			Collection<ChoiceDescription> choices,
			int defaultChoice)
		{
			if (!string.IsNullOrEmpty (caption)) {
				WriteLine (caption);
			}

			if (!string.IsNullOrEmpty (message)) {
				WriteLine (message);
			}

			int chosen = -1;
			do {
				// holds hotkeys, e.g. "[Y] Yes [N] No"
				var accelerators = new string[choices.Count];

				var promptMessage = new StringBuilder ();
				for (int index = 0; index < choices.Count; index++) {
					ChoiceDescription choice = choices[index];
					string label = choice.Label;
					int ampIndex = label.IndexOf ('&'); // hotkey marker
					accelerators[index] = string.Empty; // default to empty

					// accelerator marker found?
					if (ampIndex != -1
						&& ampIndex < label.Length - 1) {
						// grab the letter after '&'
						accelerators [index] = label
							.Substring (ampIndex + 1, 1)
							.ToUpper (CultureInfo.CurrentCulture);
					}

					promptMessage.AppendFormat (CultureInfo.CurrentCulture, "[{0}] {1}  ",
						accelerators [index],
						// remove the redundant marker from output
						label.Replace ("&", string.Empty));
				}

				promptMessage.AppendFormat (
					CultureInfo.CurrentCulture,
					"[?] Help (default is \"{0}\"):",
					accelerators [defaultChoice]);

				//WriteLine (promptMessage.ToString ());

				string input = ReadLine (promptMessage.ToString ()).Trim ();
				switch (input.Length) {
					case 0:
						// enter, accept default if provided
						if (defaultChoice == -1) {
							continue;
						}
						chosen = defaultChoice;
						break;

					case 1:
						if (input[0] == '?') {
							// show help
							for (int index = 0; index < choices.Count; index++) {
								WriteLine (string.Format (
									CultureInfo.CurrentCulture,
									"{0} - {1}.",
									accelerators[index],
									choices[index].HelpMessage));
							}
						} else {
							// single letter accelerator, e.g. "Y"
							chosen = Array.FindIndex (
								accelerators,
								accelerator => accelerator.Equals (
									input,
									StringComparison.OrdinalIgnoreCase));
						}
						break;

					default:
						// match against entire label, e.g. "Yes"
						chosen = Array.FindIndex (
							choices.ToArray (),
							choice => choice.Label.Equals (
								input,
								StringComparison.OrdinalIgnoreCase));
						break;
				}
			} while (chosen == -1);

			return chosen;
		}

		string ReadLine (string prompt)
		{
			return scriptingConsole.PromptForInput (prompt)
				.GetAwaiter ()
				.GetResult ();
		}

		void WriteLine (string message)
		{
			scriptingConsole.WriteLine (message, ScriptingStyle.Out);
		}
	}
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace NuGetConsole.Host.PowerShell
{
	/// <summary>
	/// A simple parser used for parsing commands that require completion (intellisense).
	/// </summary>
	public class CommandParser
	{
		int index;
		readonly string command;
		readonly char[] escapeChars = { 'n', 'r', 't', 'a', 'b', '"', '\'', '`', '0' };

		CommandParser (string command)
		{
			this.command = command;
		}

		char CurrentChar {
			get { return GetChar (index); }
		}

		char NextChar {
			get { return GetChar (index + 1); }
		}

		bool Done {
			get { return index >= command.Length; }
		}

		public static Command Parse (string command)
		{
			if (command == null) {
				throw new ArgumentNullException (nameof (command));
			}
			return new CommandParser (command).ParseCore ();
		}

		Command ParseCore ()
		{
			Collection<PSParseError> errors;
			Collection<PSToken> tokens = PSParser.Tokenize (command, out errors);

			// Use the powershell tokenizer to find the index of the last command so we can start parsing from there
			var lastCommandToken = tokens.LastOrDefault (t => t.Type == PSTokenType.Command);

			if (lastCommandToken != null) {
				// Start parsing from a command
				index = lastCommandToken.Start;
			}

			var parsedCommand = new Command ();
			int positionalArgumentIndex = 0;

			// Get the command name
			parsedCommand.CommandName = ParseToken ();

			while (!Done) {
				SkipWhitespace ();

				string argument = ParseToken ();

				if (argument.Length > 0
					&& argument[0] == '-') {
					// Trim the -
					argument = argument.Substring (1);

					if (!String.IsNullOrEmpty (argument)) {
						// Parse the argument value if any
						if (SkipWhitespace ()
							&& CurrentChar != '-') {
							parsedCommand.Arguments[argument] = ParseToken ();
						} else {
							parsedCommand.Arguments[argument] = null;
						}

						parsedCommand.CompletionArgument = argument;
					} else {
						// If this was an empty argument then we aren't trying to complete anything
						parsedCommand.CompletionArgument = null;
					}

					// Reset the completion index if we're completing an argument (these 2 properties are mutually exclusive)
					parsedCommand.CompletionIndex = null;
				} else {
					// Reset the completion argument
					parsedCommand.CompletionArgument = null;
					parsedCommand.CompletionIndex = positionalArgumentIndex;
					parsedCommand.Arguments[positionalArgumentIndex++] = argument;
				}
			}

			return parsedCommand;
		}

		string ParseSingleQuotes ()
		{
			var sb = new StringBuilder ();
			while (!Done) {
				sb.Append (ParseUntil (c => c == '\''));

				if (ParseChar () == '\''
					&& CurrentChar == '\'') {
					sb.Append (ParseChar ());
				} else {
					break;
				}
			}

			return sb.ToString ();
		}

		string ParseDoubleQuotes ()
		{
			var sb = new StringBuilder ();
			while (!Done) {
				// Parse until we see a quote or an escape character
				sb.Append (ParseUntil (c => c == '"' || c == '`'));

				if (IsEscapeSequence ()) {
					sb.Append (ParseChar ());
					sb.Append (ParseChar ());
				} else {
					ParseChar ();
					break;
				}
			}

			return sb.ToString ();
		}

		bool IsEscapeSequence ()
		{
			return CurrentChar == '`' && Array.IndexOf (escapeChars, NextChar) >= 0;
		}

		char ParseChar ()
		{
			char ch = CurrentChar;
			index++;
			return ch;
		}

		string ParseToken ()
		{
			if (CurrentChar == '\'') {
				ParseChar ();
				return ParseSingleQuotes ();
			}
			if (CurrentChar == '"') {
				ParseChar ();
				return ParseDoubleQuotes ();
			}
			return ParseUntil (Char.IsWhiteSpace);
		}

		string ParseUntil (Func<char, bool> predicate)
		{
			var sb = new StringBuilder ();
			while (!Done
				   && !predicate (CurrentChar)) {
				sb.Append (CurrentChar);
				index++;
			}
			return sb.ToString ();
		}

		bool SkipWhitespace ()
		{
			string ws = ParseUntil (c => !Char.IsWhiteSpace (c));
			return ws.Length > 0;
		}

		char GetChar (int index)
		{
			return index < command.Length ? command[index] : '\0';
		}
	}
}
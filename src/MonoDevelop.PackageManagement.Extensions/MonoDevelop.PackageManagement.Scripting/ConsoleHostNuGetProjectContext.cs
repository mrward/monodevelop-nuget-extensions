//
// ConsoleHostNuGetProjectContext.cs
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
using System.Xml.Linq;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement.Scripting;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.ProjectManagement;

namespace MonoDevelop.PackageManagement.Scripting
{
	class ConsoleHostNuGetProjectContext : INuGetProjectContext
	{
		NuGetProjectContext context;
		IScriptingConsole scriptingConsole;

		public ConsoleHostNuGetProjectContext (ISettings settings, FileConflictAction? conflictAction = null)
			: this (settings, conflictAction, PackageManagementExtendedServices.ConsoleHost.ScriptingConsole)
		{
		}

		ConsoleHostNuGetProjectContext (
			ISettings settings,
			FileConflictAction? conflictAction,
			IScriptingConsole scriptingConsole)
		{
			this.scriptingConsole = scriptingConsole;
			context = new NuGetProjectContext (settings);
			context.FileConflictResolution = conflictAction;
		}

		public PackageExtractionContext PackageExtractionContext {
			get { return context.PackageExtractionContext; }
			set { context.PackageExtractionContext = value;}
		}

		public ISourceControlManagerProvider SourceControlManagerProvider {
			get { return context.SourceControlManagerProvider; }
		}

		public ExecutionContext ExecutionContext {
			get { return context.ExecutionContext; }
		}

		public XDocument OriginalPackagesConfig {
			get { return context.OriginalPackagesConfig; }
			set { context.OriginalPackagesConfig = value; }
		}

		public NuGetActionType ActionType {
			get { return context.ActionType; }
			set { context.ActionType = value; }
		}

		public Guid OperationId {
			get { return context.OperationId; }
			set { context.OperationId = value; }
		}

		public void Log (MessageLevel level, string message, params object[] args)
		{
			OnBeforeScriptingConsoleWriteLine ();

			string fullMessage = string.Format (message, args);
			scriptingConsole.WriteLine (fullMessage, ToScriptStyle (level));
		}

		public void Log (ILogMessage message)
		{
			OnBeforeScriptingConsoleWriteLine ();

			scriptingConsole.WriteLine (message.Message, ToScriptStyle (message.Level));
		}

		void OnBeforeScriptingConsoleWriteLine ()
		{
			if (LogNewLineBeforeFirstMessage && !AnyMessagesLogged) {
				AnyMessagesLogged = true;
				scriptingConsole.WriteLine (string.Empty, ScriptingStyle.Out);
			}

			AnyMessagesLogged = true;
		}

		static ScriptingStyle ToScriptStyle (MessageLevel level)
		{
			switch (level) {
				case MessageLevel.Debug:
					return ScriptingStyle.Debug;
				case MessageLevel.Error:
					return ScriptingStyle.Error;
				case MessageLevel.Info:
					return ScriptingStyle.Out;
				case MessageLevel.Warning:
					return ScriptingStyle.Warning;
				default:
					return ScriptingStyle.Out;
			}
		}

		static ScriptingStyle ToScriptStyle (NuGet.Common.LogLevel level)
		{
			switch (level) {
				case NuGet.Common.LogLevel.Debug:
				case NuGet.Common.LogLevel.Minimal:
				case NuGet.Common.LogLevel.Verbose:
					return ScriptingStyle.Debug;
				case NuGet.Common.LogLevel.Error:
					return ScriptingStyle.Error;
				case NuGet.Common.LogLevel.Information:
					return ScriptingStyle.Out;
				case NuGet.Common.LogLevel.Warning:
					return ScriptingStyle.Warning;
				default:
					return ScriptingStyle.Out;
			}
		}

		public void ReportError (string message)
		{
			OnBeforeScriptingConsoleWriteLine ();

			scriptingConsole.WriteLine (message, ScriptingStyle.Error);
		}

		public void ReportError (ILogMessage message)
		{
			ReportError (message.Message);
		}

		public FileConflictAction ResolveFileConflict (string message)
		{
			if (context.FileConflictResolution.HasValue) {
				return context.FileConflictResolution.Value;
			}

			// This should be using the PowerShell console host instead of a separate GUI.
			FileConflictAction conflictAction = Runtime.RunInMainThread (() => {
				var conflictResolver = new FileConflictResolver ();
				return conflictResolver.ResolveFileConflict (message);
			}).WaitAndGetResult ();

			if (conflictAction == FileConflictAction.IgnoreAll || conflictAction == FileConflictAction.OverwriteAll) {
				context.FileConflictResolution = conflictAction;
			}

			return conflictAction;
		}

		/// <summary>
		/// Currently always true. This should be set to false if a command, such as Install-Package is not
		/// being run. This may be used to handle the case PowerShell init.ps1 scripts are run on solution
		/// open - or possibly this may just be handled in the PowerShell remote host itself.
		/// </summary>
		public bool IsExecutingPowerShellCommand { get; set; } = true;

		/// <summary>
		/// Ensures the output from any init.ps1 is not logged on the PM prompt line.
		/// Should this be handled on the console view controller side?
		/// </summary>
		public bool LogNewLineBeforeFirstMessage { get; set; }

		public bool AnyMessagesLogged { get; internal set; }
	}
}

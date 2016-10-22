// 
// PackageManagementCmdlet.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using NuGet;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	public abstract class PackageManagementCmdlet : PSCmdlet, ITerminatingCmdlet, IPackageScriptSession, IPackageScriptRunner, ILogger
	{
		IPackageManagementConsoleHost consoleHost;
		ICmdletTerminatingError terminatingError;
		
		internal PackageManagementCmdlet(
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
		{
			this.consoleHost = consoleHost;
			this.terminatingError = terminatingError;
		}
		
		internal IPackageManagementConsoleHost ConsoleHost {
			get { return consoleHost; }
		}
		
		protected Project DefaultProject {
			get { return consoleHost.DefaultProject as Project; }
		}

		protected ICmdletTerminatingError TerminatingError {
			get {
				if (terminatingError == null) {
					terminatingError = new CmdletTerminatingError(this);
				}
				return terminatingError;
			}
		}
		
		protected void ThrowErrorIfProjectNotOpen()
		{
			if (DefaultProject == null) {
				ThrowProjectNotOpenTerminatingError();
			}
		}
			
		protected void ThrowProjectNotOpenTerminatingError()
		{
			TerminatingError.ThrowNoProjectOpenError();
		}
		
		public void SetEnvironmentPath(string path)
		{
			SetSessionVariable("env:path", path);
		}
		
		protected virtual void SetSessionVariable(string name, object value)
		{
			SessionState.PSVariable.Set(name, value);
		}
		
		public string GetEnvironmentPath()
		{
			return (string)GetSessionVariable("env:path");
		}
		
		protected virtual object GetSessionVariable(string name)
		{
			return GetVariableValue(name);
		}
		
		public void AddVariable(string name, object value)
		{
			SetSessionVariable(name, value);
		}
		
		public void RemoveVariable(string name)
		{
			RemoveSessionVariable(name);
		}
		
		protected virtual void RemoveSessionVariable(string name)
		{
			SessionState.PSVariable.Remove(name);
		}
		
		public virtual void InvokeScript(string script)
		{
			var resultTypes = PipelineResultTypes.Error | PipelineResultTypes.Output;
			try {
				InvokeCommand.InvokeScript(script, false, resultTypes, null, null);
			} catch (Exception ex) {
				LoggingService.LogInternalError ("PowerShell script error.", ex);
				var errorRecord = new ErrorRecord (ex,
					"PackageManagementInternalError",
					ErrorCategory.InvalidOperation,
					null);
				WriteError (errorRecord);
			}
		}
		
		void IPackageScriptRunner.Run(IPackageScript script)
		{
			if (script.Exists()) {
				script.Run(this);
			}
		}

		protected IDisposable CreateEventsMonitor ()
		{
			return ConsoleHost.CreateEventsMonitor (this);
		}

//		internal void ExecuteWithScriptRunner (IPackageManagementProject2 project, Action action)
//		{
//			using (RunPackageScriptsAction runScriptsAction = CreateRunPackageScriptsAction (project)) {
//				action ();
//			}
//		}
//
//		RunPackageScriptsAction CreateRunPackageScriptsAction (IPackageManagementProject2 project)
//		{
//			return new RunPackageScriptsAction (this, project);
//		}

		public void Log (MessageLevel level, string message, params object[] args)
		{
			string fullMessage = String.Format (message, args);

			switch (level) {
				case MessageLevel.Error:
					WriteError (CreateErrorRecord (message));
					break;
				case MessageLevel.Warning:
					WriteWarning (fullMessage);
					break;
				case MessageLevel.Debug:
					WriteVerbose (fullMessage);
					break;
				default:
					Host.UI.WriteLine (message);
					break;
			}
		}

		ErrorRecord CreateErrorRecord (string message)
		{
			return new ErrorRecord (
				new Exception (message),
				"PackageManagementErrorId",
				ErrorCategory.NotSpecified,
				null);
		}

		public FileConflictResolution ResolveFileConflict (string message)
		{
			throw new NotImplementedException ();
		}
	}
}

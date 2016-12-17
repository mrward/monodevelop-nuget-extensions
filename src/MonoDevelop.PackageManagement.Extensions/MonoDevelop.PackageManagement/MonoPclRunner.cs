//
// MonoPclRunner.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
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
//

using System;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;

namespace MonoDevelop.PackageManagement
{
	internal class MonoPclRunner
	{
		IPackageManagementProgressMonitorFactory progressMonitorFactory;

		public MonoPclRunner ()
			: this (PackageManagementServices.ProgressMonitorFactory)
		{
		}

		public MonoPclRunner (
			IPackageManagementProgressMonitorFactory progressMonitorFactory)
		{
			this.progressMonitorFactory = progressMonitorFactory;
		}

		public void Run ()
		{
			ProgressMonitorStatusMessage progressMessage = CreateProgressStatusMessage ();
			PackageManagementProgressMonitor progressMonitor = CreateProgressMonitor (progressMessage);

			try {
				RunInternal (progressMonitor, progressMessage);
			} catch (Exception ex) {
				LoggingService.LogInternalError (ex);
				progressMonitor.Log.WriteLine (ex.Message);
				progressMonitor.ReportError (progressMessage.Error, null);
				progressMonitor.ShowPackageConsole ();
				progressMonitor.Dispose ();
			}
		}

		ProgressMonitorStatusMessage CreateProgressStatusMessage ()
		{
			return new ProgressMonitorStatusMessage (
				"Looking for portable class libraries...",
				String.Empty,
				"Failed to find portable class libraries.",
				String.Empty
			);
		}

		PackageManagementProgressMonitor CreateProgressMonitor (ProgressMonitorStatusMessage progressMessage)
		{
			return (PackageManagementProgressMonitor)progressMonitorFactory.CreateProgressMonitor (progressMessage.Status);
		}

		void RunInternal (PackageManagementProgressMonitor progressMonitor, ProgressMonitorStatusMessage progressMessage)
		{
			var commandLine = new MonoPclCommandLine () {
				List = true
			};
			commandLine.BuildCommandLine ();

			progressMonitor.ShowPackageConsole ();
			progressMonitor.Log.WriteLine (commandLine.ToString ());
			progressMonitor.Log.WriteLine ();

			RunMonoPcl (progressMonitor, progressMessage, commandLine);
		}

		void RunMonoPcl (
			PackageManagementProgressMonitor progressMonitor,
			ProgressMonitorStatusMessage progressMessage,
			MonoPclCommandLine commandLine)
		{
			Runtime.ProcessService.StartConsoleProcess (
				commandLine.Command,
				commandLine.Arguments,
				commandLine.WorkingDirectory,
				progressMonitor.Console,
				null,
				(sender, e) => {
					using (progressMonitor) {
						ReportOutcome ((ProcessAsyncOperation)sender, progressMonitor, progressMessage);
					}
				}
			);
		}

		void ReportOutcome (
			ProcessAsyncOperation operation,
			ProgressMonitor progressMonitor,
			ProgressMonitorStatusMessage progressMessage)
		{
			if (!operation.Task.IsFaulted && operation.ExitCode == 0) {
				progressMonitor.ReportSuccess (progressMessage.Success);
			} else {
				progressMonitor.ReportError (progressMessage.Error, null);
				progressMonitor.ShowPackageConsole ();
			}
		}
	}
}


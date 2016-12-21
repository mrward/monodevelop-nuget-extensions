//
// ConsoleHostPackageEventsMonitor.cs
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
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Core;
using NuGet.ProjectManagement;

namespace MonoDevelop.PackageManagement
{
	internal class ConsoleHostPackageEventsMonitor : IDisposable
	{
		IPackageManagementEvents packageManagementEvents;
		INuGetProjectContext context;
		List<FileEventArgs> fileChangedEvents = new List<FileEventArgs> ();

		public ConsoleHostPackageEventsMonitor (INuGetProjectContext context)
			: this (
				context,
				PackageManagementServices.PackageManagementEvents)
		{
		}

		public ConsoleHostPackageEventsMonitor (
			INuGetProjectContext context,
			IPackageManagementEvents packageManagementEvents)
		{
			this.packageManagementEvents = packageManagementEvents;
			this.context = context;

			packageManagementEvents.FileChanged += FileChanged;
			packageManagementEvents.PackageOperationMessageLogged += PackageOperationMessageLogged;
		}

		public void Dispose ()
		{
			packageManagementEvents.OnPackageOperationsFinished ();
 
			packageManagementEvents.PackageOperationMessageLogged -= PackageOperationMessageLogged;
			packageManagementEvents.FileChanged -= FileChanged;
			NotifyFilesChanged ();
		}

		void PackageOperationMessageLogged (object sender, PackageOperationMessageLoggedEventArgs e)
		{
			context.Log (e.Message.Level, e.Message.ToString ());
		}

		void FileChanged (object sender, FileEventArgs e)
		{
			fileChangedEvents.Add (e);
		}

		void NotifyFilesChanged ()
		{
			Runtime.RunInMainThread (() => {
				FilePath[] files = fileChangedEvents
					.SelectMany (fileChangedEvent => fileChangedEvent.ToArray ())
					.Select (fileInfo => fileInfo.FileName)
					.ToArray ();

				if (files.Any ())
					NotifyFilesChanged (files);
			});
		}

		void NotifyFilesChanged (FilePath[] files)
		{
			FileService.NotifyFilesChanged (files);
		}
	}
}


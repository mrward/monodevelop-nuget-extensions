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
using NuGet;

namespace MonoDevelop.PackageManagement
{
	internal class ConsoleHostPackageEventsMonitor : IDisposable
	{
		IPackageManagementEvents packageManagementEvents;
		ILogger logger;
		List<FileEventArgs> fileChangedEvents = new List<FileEventArgs> ();

		public ConsoleHostPackageEventsMonitor (ILogger logger)
			: this (
				logger,
				PackageManagementServices.PackageManagementEvents)
		{
		}

		public ConsoleHostPackageEventsMonitor (
			ILogger logger,
			IPackageManagementEvents packageManagementEvents)
		{
			this.packageManagementEvents = packageManagementEvents;
			this.logger = logger;

			packageManagementEvents.FileChanged += FileChanged;
			packageManagementEvents.PackageOperationMessageLogged += PackageOperationMessageLogged;
		}

		public void Dispose ()
		{
			packageManagementEvents.PackageOperationMessageLogged -= PackageOperationMessageLogged;
			packageManagementEvents.FileChanged -= FileChanged;
			NotifyFilesChanged ();
		}

		void PackageOperationMessageLogged (object sender, PackageOperationMessageLoggedEventArgs e)
		{
			logger.Log (e.Message.Level, e.Message.ToString ());
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

				NotifyFilesChanged (files);
			});
		}

		void NotifyFilesChanged (FilePath[] files)
		{
			FileService.NotifyFilesChanged (files);
		}
	}
}


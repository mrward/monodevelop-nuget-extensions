// 
// ManagePackagesUserPrompts.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2013 Matthew Ward
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
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.PackageManagement;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using NuGet;

namespace MonoDevelop.PackageManagement
{
	internal class ManagePackagesUserPrompts : IDisposable
	{
		ILicenseAcceptanceService licenseAcceptanceService;
		ISelectProjectsService selectProjectsService;
		IPackageManagementEvents packageManagementEvents;
		IFileConflictResolver fileConflictResolver;
		FileConflictResolution lastFileConflictResolution;
		List<FileEventArgs> fileChangedEvents = new List<FileEventArgs> ();

		public ManagePackagesUserPrompts (IPackageManagementEvents packageManagementEvents)
			: this (
				packageManagementEvents,
				new LicenseAcceptanceService2 (),
				new SelectProjectsService (),
				new FileConflictResolver ())
		{
		}

		public ManagePackagesUserPrompts (
			IPackageManagementEvents packageManagementEvents,
			ILicenseAcceptanceService licenseAcceptanceService,
			ISelectProjectsService selectProjectsService,
			IFileConflictResolver fileConflictResolver)
		{
			this.packageManagementEvents = packageManagementEvents;
			this.licenseAcceptanceService = licenseAcceptanceService;
			this.selectProjectsService = selectProjectsService;
			this.fileConflictResolver = fileConflictResolver;

			ResetFileConflictResolution ();
			SubscribeToEvents ();
		}

		void ResetFileConflictResolution ()
		{
			lastFileConflictResolution = FileConflictResolution.Overwrite;
		}

		void SubscribeToEvents ()
		{
			packageManagementEvents.AcceptLicenses += AcceptLicenses;
			packageManagementEvents.SelectProjects += SelectProjects;
			packageManagementEvents.ResolveFileConflict += ResolveFileConflict;
			packageManagementEvents.PackageOperationsStarting += PackageOperationsStarting;
			packageManagementEvents.FileChanged += FileChanged;
		}

		void AcceptLicenses (object sender, AcceptLicensesEventArgs e)
		{
			Runtime.RunInMainThread (() => {
				e.IsAccepted = licenseAcceptanceService.AcceptLicenses (e.Packages);
			}).Wait ();
		}

		void SelectProjects (object sender, SelectProjectsEventArgs e)
		{
			Runtime.RunInMainThread (() => {
				e.IsAccepted = selectProjectsService.SelectProjects (e.SelectedProjects);
			}).Wait ();
		}

		void ResolveFileConflict (object sender, ResolveFileConflictEventArgs e)
		{
			if (UserPreviouslySelectedOverwriteAllOrIgnoreAll ()) {
				e.Resolution = lastFileConflictResolution;
			} else {
				e.Resolution = fileConflictResolver.ResolveFileConflict (e.Message);
				lastFileConflictResolution = e.Resolution;
			}
		}

		bool UserPreviouslySelectedOverwriteAllOrIgnoreAll ()
		{
			return
				(lastFileConflictResolution == FileConflictResolution.IgnoreAll) ||
				(lastFileConflictResolution == FileConflictResolution.OverwriteAll);
		}

		void PackageOperationsStarting (object sender, EventArgs e)
		{
			ResetFileConflictResolution ();
		}

		public void Dispose ()
		{
			UnsubscribeFromEvents ();
			NotifyFilesChanged ();
		}

		public void UnsubscribeFromEvents ()
		{
			packageManagementEvents.SelectProjects -= SelectProjects;
			packageManagementEvents.AcceptLicenses -= AcceptLicenses;
			packageManagementEvents.ResolveFileConflict -= ResolveFileConflict;
			packageManagementEvents.PackageOperationsStarting -= PackageOperationsStarting;
			packageManagementEvents.FileChanged -= FileChanged;
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

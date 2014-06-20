// 
// PackagesViewModels.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2013-2014 Matthew Ward
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
using ICSharpCode.PackageManagement;

namespace MonoDevelop.PackageManagement
{
	public class PackagesViewModels2 : IDisposable
	{
		public PackagesViewModels2 (
			IPackageManagementSolution solution,
			IRegisteredPackageRepositories registeredPackageRepositories,
			IThreadSafePackageManagementEvents packageManagementEvents,
			IPackageActionRunner actionRunner,
			ITaskFactory taskFactory)
		{
			var packageViewModelFactory = new PackageViewModelFactory2 (solution, packageManagementEvents, actionRunner);
			var updatedPackageViewModelFactory = new UpdatedPackageViewModelFactory2 (packageViewModelFactory);
			var installedPackageViewModelFactory = new InstalledPackageViewModelFactory2 (packageViewModelFactory);

			IRecentPackageRepository recentPackageRepository = PackageManagementServices.RecentPackageRepository;
			AvailablePackagesViewModel = new AvailablePackagesViewModel2 (registeredPackageRepositories, recentPackageRepository, packageViewModelFactory, taskFactory);
			InstalledPackagesViewModel = new InstalledPackagesViewModel2 (solution, packageManagementEvents, registeredPackageRepositories, installedPackageViewModelFactory, taskFactory);
			UpdatedPackagesViewModel = new UpdatedPackagesViewModel2 (solution, registeredPackageRepositories, updatedPackageViewModelFactory, taskFactory);
			RecentPackagesViewModel = new RecentPackagesViewModel2 (packageManagementEvents, registeredPackageRepositories, packageViewModelFactory, taskFactory);
		}

		public AvailablePackagesViewModel2 AvailablePackagesViewModel { get; private set; }
		public InstalledPackagesViewModel2 InstalledPackagesViewModel { get; private set; }
		public RecentPackagesViewModel2 RecentPackagesViewModel { get; private set; }
		public UpdatedPackagesViewModel2 UpdatedPackagesViewModel { get; private set; }

		public void ReadPackages ()
		{
			AvailablePackagesViewModel.ReadPackages ();
			InstalledPackagesViewModel.ReadPackages ();
			UpdatedPackagesViewModel.ReadPackages ();
			RecentPackagesViewModel.ReadPackages ();
		}

		public void Dispose ()
		{
			AvailablePackagesViewModel.Dispose ();
			InstalledPackagesViewModel.Dispose ();
			RecentPackagesViewModel.Dispose ();
			UpdatedPackagesViewModel.Dispose ();
		}
	}
}

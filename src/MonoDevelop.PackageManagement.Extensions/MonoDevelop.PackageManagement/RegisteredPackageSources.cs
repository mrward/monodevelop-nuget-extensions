//
// RegisteredPackageSources.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
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
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

namespace MonoDevelop.PackageManagement
{
	class RegisteredPackageSources
	{
		IMonoDevelopSolutionManager solutionManager;
		List<SourceRepositoryViewModel> packageSources;
		IPackageSourceProvider packageSourceProvider;
		SourceRepositoryViewModel selectedPackageSource;

		public RegisteredPackageSources (IMonoDevelopSolutionManager solutionManager)
		{
			this.solutionManager = solutionManager;
		}

		public IEnumerable<SourceRepositoryViewModel> PackageSources {
			get {
				if (packageSources == null) {
					packageSources = GetPackageSources ().ToList ();
				}
				return packageSources;
			}
		}

		IEnumerable<SourceRepositoryViewModel> GetPackageSources ()
		{
			ISourceRepositoryProvider provider = solutionManager.CreateSourceRepositoryProvider ();
			packageSourceProvider = provider.PackageSourceProvider;
			var repositories = provider.GetRepositories ().ToList ();

			if (repositories.Count > 1) {
				yield return new AggregateSourceRepositoryViewModel (repositories);
			}

			foreach (SourceRepository repository in repositories) {
				yield return new SourceRepositoryViewModel (repository);
			}
		}

		public SourceRepositoryViewModel SelectedPackageSource {
			get {
				if (selectedPackageSource == null) {
					selectedPackageSource = GetActivePackageSource ();
				}
				return selectedPackageSource;
			}
			set {
				if (selectedPackageSource != value) {
					selectedPackageSource = value;
					SaveActivePackageSource ();
				}
			}
		}

		SourceRepositoryViewModel GetActivePackageSource ()
		{
			if (packageSources == null)
				return null;

			if (!string.IsNullOrEmpty (packageSourceProvider.ActivePackageSourceName)) {
				SourceRepositoryViewModel packageSource = packageSources
					.FirstOrDefault (viewModel => string.Equals (viewModel.PackageSource.Name, packageSourceProvider.ActivePackageSourceName, StringComparison.CurrentCultureIgnoreCase));
				if (packageSource != null) {
					return packageSource;
				}
			}

			return packageSources.FirstOrDefault (packageSource => !packageSource.IsAggregate);
		}

		void SaveActivePackageSource ()
		{
			if (selectedPackageSource == null || packageSourceProvider == null)
				return;

			packageSourceProvider.SaveActivePackageSource (selectedPackageSource.PackageSource);
		}
	}
}

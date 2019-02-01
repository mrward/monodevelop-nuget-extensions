//
// SourceRepositoryProviderExtensions.cs
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

using System.Collections.Generic;
using System.Linq;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using NuGet.Protocol.Core.Types;

namespace MonoDevelop.PackageManagement.Protocol
{
	static class SourceRepositoryProviderExtensions
	{
		public static IEnumerable<SourceRepository> GetRepositories (
			this ISourceRepositoryProvider repositoryProvider,
			IEnumerable<PackageSourceInfo> sources)
		{
			var allRepositories = repositoryProvider.GetRepositories ().ToList ();

			var repositories = new List<SourceRepository> ();
			foreach (PackageSourceInfo source in sources) {
				var packageSource = new NuGet.Configuration.PackageSource (source.Source, source.Name);
				SourceRepository matchedRepository = FindSourceRepository (packageSource, allRepositories);
				if (matchedRepository != null) {
					repositories.Add (matchedRepository);
				} else {
					var repository = repositoryProvider.CreateRepository (packageSource);
					repositories.Add (repository);
				}
			}

			return repositories;
		}

		static SourceRepository FindSourceRepository (NuGet.Configuration.PackageSource source, List<SourceRepository> repositories)
		{
			foreach (SourceRepository repository in repositories) {
				if (repository.PackageSource.Equals (source)) {
					return repository;
				}
			}
			return null;
		}

		public static IEnumerable<SourceRepository> GetEnabledRepositories (this ISourceRepositoryProvider repositoryProvider)
		{
			return repositoryProvider.GetRepositories()
				.Where (repository => repository.PackageSource.IsEnabled)
				.ToList ();
		}
	}
}

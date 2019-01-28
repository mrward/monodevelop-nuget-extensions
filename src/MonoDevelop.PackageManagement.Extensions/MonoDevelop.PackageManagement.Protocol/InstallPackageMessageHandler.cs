//
// InstallPackageMessageHandler.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace MonoDevelop.PackageManagement.Protocol
{
	class InstallPackageMessageHandler
	{
		DotNetProject project;
		InstallPackageParams message;
		IMonoDevelopSolutionManager solutionManager;
		NuGetProject nugetProject;
		MonoDevelopNuGetPackageManager packageManager;
		NuGetProjectContext projectContext;
		PackageIdentity identity;

		public InstallPackageMessageHandler (DotNetProject project, InstallPackageParams message)
		{
			this.project = project;
			this.message = message;
		}

		public Task<IEnumerable<NuGetProjectAction>> PreviewInstallPackage (
			CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				return PreviewInstallPackage (token, sourceCacheContext);
			}
		}

		public async Task<IEnumerable<NuGetProjectAction>> PreviewInstallPackage (
			CancellationToken token,
			SourceCacheContext sourceCacheContext)
		{
			solutionManager = project.GetSolutionManager ();
			nugetProject = project.CreateNuGetProject (solutionManager);

			packageManager = new MonoDevelopNuGetPackageManager (solutionManager);
			var repositories = GetSourceRepositories (message.PackageSources);

			projectContext = new NuGetProjectContext (solutionManager.Settings);
			var dependencyBehavior = (DependencyBehavior)Enum.Parse (typeof (DependencyBehavior), message.DependencyBehavior);

			NuGetVersion version = null;
			if (string.IsNullOrEmpty (message.PackageVersion)) {
				version = await GetLatestPackageVersion (repositories, dependencyBehavior, sourceCacheContext);
			} else {
				version = NuGetVersion.Parse (message.PackageVersion);
			}

			identity = new PackageIdentity (message.PackageId, version);

			var resolutionContext = new ResolutionContext (
				dependencyBehavior,
				message.AllowPrerelease,
				true,
				VersionConstraints.None,
				new GatherCache (),
				sourceCacheContext
			);

			return await packageManager.PreviewInstallPackageAsync (
				nugetProject,
				identity,
				resolutionContext,
				projectContext,
				repositories,
				null,
				token
			).ConfigureAwait (false);
		}

		static IEnumerable<SourceRepository> GetSourceRepositories (IEnumerable<PackageSourceInfo> sources)
		{
			var repositoryProvider = SourceRepositoryProviderFactory.CreateSourceRepositoryProvider ();
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

		async Task<NuGetVersion> GetLatestPackageVersion (
			IEnumerable<SourceRepository> repositories,
			DependencyBehavior dependencyBehavior,
			SourceCacheContext sourceCacheContext)
		{
			var latestVersionContext = new ResolutionContext (
				dependencyBehavior,
				message.AllowPrerelease,
				false,
				VersionConstraints.None,
				new GatherCache (),
				sourceCacheContext
			);

			ResolvedPackage resolvedPackage = await packageManager.GetLatestVersionAsync (
				message.PackageId,
				nugetProject,
				latestVersionContext,
				repositories,
				new ProjectContextLogger (projectContext),
				CancellationToken.None);

			if (resolvedPackage?.LatestVersion == null) {
				throw new InvalidOperationException (GettextCatalog.GetString ("Unable to find package '{0}", message.PackageId));
			}

			return resolvedPackage?.LatestVersion;
		}

		public async Task InstallPackageAsync (CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				var actions = await PreviewInstallPackage (token, sourceCacheContext);

				NuGetPackageManager.SetDirectInstall (identity, projectContext);

				await packageManager.ExecuteNuGetProjectActionsAsync (
					nugetProject,
					actions,
					projectContext,
					sourceCacheContext,
					CancellationToken.None);

				NuGetPackageManager.ClearDirectInstall (projectContext);
			}
		}
	}
}

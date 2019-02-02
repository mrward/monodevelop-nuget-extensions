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
		ConsoleHostNuGetProjectContext projectContext;
		PackageIdentity identity;

		public bool IsPackageAlreadyInstalled { get; private set; }

		public InstallPackageMessageHandler (DotNetProject project, InstallPackageParams message)
		{
			this.project = project;
			this.message = message;
		}

		public Task<IEnumerable<NuGetProjectAction>> PreviewInstallPackageAsync (
			CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				return PreviewInstallPackage (sourceCacheContext, token);
			}
		}

		public async Task<IEnumerable<NuGetProjectAction>> PreviewInstallPackage (
			SourceCacheContext sourceCacheContext,
			CancellationToken token)
		{
			solutionManager = project.GetSolutionManager ();
			nugetProject = project.CreateNuGetProject (solutionManager);

			packageManager = new MonoDevelopNuGetPackageManager (solutionManager);

			var repositoryProvider = solutionManager.CreateSourceRepositoryProvider ();
			var repositories = repositoryProvider.GetRepositories (message.PackageSources);

			projectContext = new ConsoleHostNuGetProjectContext (
				solutionManager.Settings,
				 message.FileConflictAction.ToFileConflictActionEnum ());

			var dependencyBehavior = message.DependencyBehavior.ToDependencyBehaviorEnum ();

			NuGetVersion version = null;
			if (string.IsNullOrEmpty (message.PackageVersion)) {
				version = await GetLatestPackageVersion (repositories, dependencyBehavior, sourceCacheContext, token);
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

			try {
				return await packageManager.PreviewInstallPackageAsync (
					nugetProject,
					identity,
					resolutionContext,
					projectContext,
					repositories,
					null,
					token
				).ConfigureAwait (false);
			} catch (InvalidOperationException ex) {
				if (ex.InnerException is PackageAlreadyInstalledException) {
					IsPackageAlreadyInstalled = true;
					return Enumerable.Empty<NuGetProjectAction> ();
				} else {
					throw;
				}
			}
		}

		async Task<NuGetVersion> GetLatestPackageVersion (
			IEnumerable<SourceRepository> repositories,
			DependencyBehavior dependencyBehavior,
			SourceCacheContext sourceCacheContext,
			CancellationToken token)
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
				token);

			if (resolvedPackage?.LatestVersion == null) {
				throw new InvalidOperationException (GettextCatalog.GetString ("Unable to find package '{0}", message.PackageId));
			}

			return resolvedPackage?.LatestVersion;
		}

		public async Task InstallPackageAsync (CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				var actions = await PreviewInstallPackage (sourceCacheContext, token);

				if (!actions.Any ()) {
					return;
				}

				NuGetPackageManager.SetDirectInstall (identity, projectContext);

				await packageManager.ExecuteNuGetProjectActionsAsync (
					nugetProject,
					actions,
					projectContext,
					sourceCacheContext,
					token);

				NuGetPackageManager.ClearDirectInstall (projectContext);
			}
		}
	}
}

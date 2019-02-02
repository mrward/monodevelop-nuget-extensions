//
// UpdatePackageMessageHandler.cs
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
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace MonoDevelop.PackageManagement.Protocol
{
	class UpdatePackageMessageHandler
	{
		List<DotNetProject> projects;
		UpdatePackageParams message;
		IMonoDevelopSolutionManager solutionManager;
		List<NuGetProject> nugetProjects;
		NuGetPackageManager packageManager;
		ConsoleHostNuGetProjectContext projectContext;

		public UpdatePackageMessageHandler (IEnumerable<DotNetProject> projects, UpdatePackageParams message)
		{
			this.projects = projects.ToList ();
			this.message = message;
		}

		public bool IsPackageInstalled { get; private set; }

		public async Task<IEnumerable<NuGetProjectAction>> PreviewUpdatePackageAsync (
			CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				return await PreviewUpdatePackagesAsync (sourceCacheContext, token);
			}
		}

		async Task<IEnumerable<NuGetProjectAction>> PreviewUpdatePackagesAsync (
			SourceCacheContext sourceCacheContext,
			CancellationToken token)
		{
			solutionManager = projects.FirstOrDefault ().GetSolutionManager ();
			nugetProjects = projects
				.Select (project => project.CreateNuGetProject (solutionManager))
				.ToList ();

			IsPackageInstalled = await CheckPackageInstalled (token);
			if (!IsPackageInstalled) {
				return Enumerable.Empty<NuGetProjectAction> ();
			}

			var repositoryProvider = solutionManager.CreateSourceRepositoryProvider ();

			packageManager = new NuGetPackageManager (
				repositoryProvider,
				solutionManager.Settings,
				solutionManager,
				new DeleteOnRestartManager ());

			var enabledRepositories = repositoryProvider.GetEnabledRepositories ();

			var repositories = repositoryProvider.GetRepositories (message.PackageSources);

			projectContext = new ConsoleHostNuGetProjectContext (
				solutionManager.Settings,
				message.FileConflictAction.ToFileConflictActionEnum ());

			var resolutionContext = new ResolutionContext (
				message.DependencyBehavior.ToDependencyBehaviorEnum (),
				message.AllowPrerelease,
				false,
				message.VersionConstraints.ToVersionContrainsEnum (),
				new GatherCache (),
				sourceCacheContext
			);

			if (!string.IsNullOrEmpty (message.PackageVersion)) {
				var packageVersion = NuGetVersion.Parse (message.PackageVersion);

				return await packageManager.PreviewUpdatePackagesAsync (
					new PackageIdentity (message.PackageId, packageVersion),
					nugetProjects,
					resolutionContext,
					projectContext,
					repositories,
					enabledRepositories,
					token
				).ConfigureAwait (false);
			} else {
				return await packageManager.PreviewUpdatePackagesAsync (
					message.PackageId,
					nugetProjects,
					resolutionContext,
					projectContext,
					repositories,
					enabledRepositories,
					token
				).ConfigureAwait (false);
			}
		}

		async Task<bool> CheckPackageInstalled (CancellationToken token)
		{
			foreach (NuGetProject nugetProject in nugetProjects) {
				var installedPackages = await nugetProject.GetInstalledPackagesAsync (token);
				if (installedPackages.Any (package => IsMatch (message.PackageId, package))) {
					return true;
				}
			}
			return false;
		}

		bool IsMatch (string packageId, PackageReference package)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (packageId, package.PackageIdentity.Id);
		}

		public async Task UpdatePackageAsync (CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				var actions = await PreviewUpdatePackagesAsync (sourceCacheContext, token);

				await packageManager.ExecuteNuGetProjectActionsAsync (
					nugetProjects,
					actions,
					projectContext,
					sourceCacheContext,
					token);
			}
		}

		/// <summary>
		/// NuGet's Update-Command when updating all packages only uses the sources selected in the
		/// PowerShell console. This is different to how updating a single package works. Not sure
		/// why this is done.
		/// </summary>
		public async Task UpdateAllPackagesAsync (CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				var actions = await PreviewUpdateAllPackagesAsync (sourceCacheContext, token);

				await packageManager.ExecuteNuGetProjectActionsAsync (
					nugetProjects,
					actions,
					projectContext,
					sourceCacheContext,
					token);
			}
		}

		async Task<IEnumerable<NuGetProjectAction>> PreviewUpdateAllPackagesAsync (
			SourceCacheContext sourceCacheContext,
			CancellationToken token)
		{
			solutionManager = projects.FirstOrDefault ().GetSolutionManager ();
			nugetProjects = projects
				.Select (project => project.CreateNuGetProject (solutionManager))
				.ToList ();

			var repositoryProvider = solutionManager.CreateSourceRepositoryProvider ();

			packageManager = new NuGetPackageManager (
				repositoryProvider,
				solutionManager.Settings,
				solutionManager,
				new DeleteOnRestartManager ());

			var repositories = repositoryProvider.GetRepositories (message.PackageSources);

			projectContext = new ConsoleHostNuGetProjectContext (
				solutionManager.Settings,
				message.FileConflictAction.ToFileConflictActionEnum ());

			var resolutionContext = new ResolutionContext (
				message.DependencyBehavior.ToDependencyBehaviorEnum (),
				message.AllowPrerelease,
				false,
				message.VersionConstraints.ToVersionContrainsEnum (),
				new GatherCache (),
				sourceCacheContext
			);

			return await packageManager.PreviewUpdatePackagesAsync (
				nugetProjects,
				resolutionContext,
				projectContext,
				repositories,
				repositories, // Update-Package does not use all enabled package sources.
				token
			).ConfigureAwait (false);
		}
	}
}

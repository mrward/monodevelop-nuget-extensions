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
using NuGet.Resolver;
using NuGet.Versioning;

namespace MonoDevelop.PackageManagement.Protocol
{
	class UpdatePackageMessageHandler
	{
		DotNetProject project;
		UpdatePackageParams message;
		IMonoDevelopSolutionManager solutionManager;
		NuGetProject nugetProject;
		NuGetPackageManager packageManager;
		NuGetProjectContext projectContext;

		public UpdatePackageMessageHandler (DotNetProject project, UpdatePackageParams message)
		{
			this.project = project;
			this.message = message;
		}

		public bool IsPackageInstalled { get; private set; }

		public async Task<IEnumerable<NuGetProjectAction>> PreviewUpdatePackage (
			CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				return await PreviewUpdatePackage (token, sourceCacheContext);
			}
		}

		async Task<IEnumerable<NuGetProjectAction>> PreviewUpdatePackage (
			CancellationToken token,
			SourceCacheContext sourceCacheContext)
		{
			solutionManager = project.GetSolutionManager ();
			nugetProject = project.CreateNuGetProject (solutionManager);

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

			projectContext = new NuGetProjectContext (solutionManager.Settings);

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
					new[] { nugetProject },
					resolutionContext,
					projectContext,
					repositories,
					enabledRepositories,
					token
				).ConfigureAwait (false);
			} else {
				return await packageManager.PreviewUpdatePackagesAsync (
					message.PackageId,
					new [] { nugetProject },
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
			var installedPackages = await nugetProject.GetInstalledPackagesAsync (token);
			return installedPackages.Any (package => IsMatch (message.PackageId, package));
		}

		bool IsMatch (string packageId, PackageReference package)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (packageId, package.PackageIdentity.Id);
		}

		public async Task UpdatePackageAsync (CancellationToken token)
		{
			using (var sourceCacheContext = new SourceCacheContext ()) {
				var actions = await PreviewUpdatePackage (token, sourceCacheContext);
				if (!actions.Any ()) {
					return;
				}

				SetDirectInstall (actions);

				await packageManager.ExecuteNuGetProjectActionsAsync (
					nugetProject,
					actions,
					projectContext,
					sourceCacheContext,
					token);

				NuGetPackageManager.ClearDirectInstall (projectContext);
			}
		}

		void SetDirectInstall (IEnumerable<NuGetProjectAction> actions)
		{
			var matchedAction = actions.FirstOrDefault (action => IsInstallActionForPackageBeingUpdated (action));
			if (matchedAction != null) {
				NuGetPackageManager.SetDirectInstall (matchedAction.PackageIdentity, projectContext);
			}
		}

		bool IsInstallActionForPackageBeingUpdated (NuGetProjectAction action)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (action.PackageIdentity.Id, message.PackageId) &&
				action.NuGetProjectActionType == NuGetProjectActionType.Install;
		}
	}
}

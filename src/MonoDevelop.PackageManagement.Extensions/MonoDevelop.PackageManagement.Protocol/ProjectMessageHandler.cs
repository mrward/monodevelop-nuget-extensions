﻿//
// ProjectMessageHandler.cs
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
using Newtonsoft.Json.Linq;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.Protocol
{
	class ProjectMessageHandler
	{
		[JsonRpcMethod (Methods.ProjectInstalledPackagesName)]
		public ProjectPackagesList OnGetInstalledPackages (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectParams> ();
				var project = FindProject (message.FileName);
				var packages = GetInstalledPackages (project).Result;
				return new ProjectPackagesList {
					Packages = CreatePackageInformation (packages).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetInstalledPackages error: {0}", ex);
				throw;
			}
		}

		async Task<IEnumerable<PackageReference>> GetInstalledPackages (DotNetProject project)
		{
			var solutionManager = GetSolutionManager (project);
			var nugetProject = CreateNuGetProject (solutionManager, project);

			return await Task.Run (() => nugetProject.GetInstalledPackagesAsync (CancellationToken.None)).ConfigureAwait (false);
		}

		static IMonoDevelopSolutionManager GetSolutionManager (DotNetProject project)
		{
			return Runtime.RunInMainThread (() => {
				return PackageManagementServices.Workspace.GetSolutionManager (project.ParentSolution);
			}).Result;
		}

		static NuGetProject CreateNuGetProject (IMonoDevelopSolutionManager solutionManager, DotNetProject project)
		{
			if (solutionManager != null) {
				return solutionManager.GetNuGetProject (new DotNetProjectProxy (project));
			}

			return new MonoDevelopNuGetProjectFactory ().CreateNuGetProject (project);
		}

		IEnumerable<PackageReferenceInfo> CreatePackageInformation (IEnumerable<PackageReference> packages)
		{
			return packages.Select (package => CreatePackageInformation (package));
		}

		PackageReferenceInfo CreatePackageInformation (PackageReference package)
		{
			return new PackageReferenceInfo {
				Id = package.PackageIdentity.Id,
				Version = package.PackageIdentity.Version.ToNormalizedString (),
				VersionRange = package.AllowedVersions?.ToNormalizedString (),
				TargetFramework = package.TargetFramework.ToString (),
				IsDevelopmentDependency = package.IsDevelopmentDependency,
				IsUserInstalled = package.IsUserInstalled,
				RequireReinstallation = package.RequireReinstallation
			};
		}

		DotNetProject FindProject (string fileName)
		{
			var matchedProject = PackageManagementServices
				.ProjectService
				.GetOpenProjects ()
				.FirstOrDefault (project => project.FileName == fileName);

			return matchedProject.DotNetProject;
		}

		[JsonRpcMethod (Methods.ProjectPreviewUninstallPackage)]
		public PackageActionList OnPreviewUninstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<UninstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				var actions = PreviewUninstallPackage (project, message).Result;
				return new PackageActionList {
					Actions = CreatePackageActionInformation (actions).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnPreviewUninstallPackage error: {0}", ex);
				throw;
			}
		}

		async Task<IEnumerable<NuGetProjectAction>> PreviewUninstallPackage (DotNetProject project, UninstallPackageParams message)
		{
			var solutionManager = GetSolutionManager (project);
			var nugetProject = CreateNuGetProject (solutionManager, project);

			var packageManager = new MonoDevelopNuGetPackageManager (solutionManager);
			var uninstallationContext = new UninstallationContext (message.RemoveDependencies, message.Force);
			var context = new NuGetProjectContext (solutionManager.Settings);

			return await packageManager.PreviewUninstallPackageAsync (
				nugetProject,
				message.PackageId,
				uninstallationContext,
				context,
				CancellationToken.None
			).ConfigureAwait (false);
		}

		IEnumerable<PackageActionInfo> CreatePackageActionInformation (IEnumerable<NuGetProjectAction> actions)
		{
			return actions.Select (action => CreatePackageActionInformation (action));
		}

		PackageActionInfo CreatePackageActionInformation (NuGetProjectAction package)
		{
			return new PackageActionInfo {
				PackageId = package.PackageIdentity.Id,
				PackageVersion = package.PackageIdentity.Version.ToNormalizedString (),
				Type = package.NuGetProjectActionType.ToString ()
			};
		}

		[JsonRpcMethod (Methods.ProjectUninstallPackage)]
		public void OnUninstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<UninstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				UninstallPackageAsync (project, message).Wait ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnUninstallPackage error: {0}", ex);
				throw;
			}
		}

		async Task UninstallPackageAsync (DotNetProject project, UninstallPackageParams message)
		{
			var solutionManager = GetSolutionManager (project);
			var nugetProject = CreateNuGetProject (solutionManager, project);

			var packageManager = new MonoDevelopNuGetPackageManager (solutionManager);
			var uninstallationContext = new UninstallationContext (message.RemoveDependencies, message.Force);
			var context = new NuGetProjectContext (solutionManager.Settings);

			var actions = await packageManager.PreviewUninstallPackageAsync (
				nugetProject,
				message.PackageId,
				uninstallationContext,
				context,
				CancellationToken.None
			).ConfigureAwait (false);

			await packageManager.ExecuteNuGetProjectActionsAsync (
				nugetProject,
				actions,
				context,
				NullSourceCacheContext.Instance,
				CancellationToken.None
			).ConfigureAwait (false);
		}

		[JsonRpcMethod (Methods.ProjectPreviewInstallPackage)]
		public PackageActionList OnPreviewInstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<InstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				var actions = PreviewInstallPackage (project, message).Result;
				return new PackageActionList {
					Actions = CreatePackageActionInformation (actions).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnPreviewUninstallPackage error: {0}", ex);
				throw;
			}
		}

		async Task<IEnumerable<NuGetProjectAction>> PreviewInstallPackage (DotNetProject project, InstallPackageParams message)
		{
			var solutionManager = GetSolutionManager (project);
			var nugetProject = CreateNuGetProject (solutionManager, project);

			var packageManager = new MonoDevelopNuGetPackageManager (solutionManager);
			var repositories = GetSourceRepositories (message.PackageSources);

			var context = new NuGetProjectContext (solutionManager.Settings);
			var dependencyBehavior = (DependencyBehavior)Enum.Parse (typeof (DependencyBehavior), message.DependencyBehavior);

			using (var sourceCacheContext = new SourceCacheContext ()) {

				NuGetVersion version = null;
				if (string.IsNullOrEmpty (message.PackageVersion)) {
					version = await GetLatestPackageVersion (message, nugetProject, packageManager, repositories, context, dependencyBehavior, sourceCacheContext);
				} else {
					version = NuGetVersion.Parse (message.PackageVersion);
				}

				var resolutionContext = new ResolutionContext (
					dependencyBehavior,
					message.AllowPrerelease,
					true,
					VersionConstraints.None,
					new GatherCache (),
					sourceCacheContext
				);

				var identity = new PackageIdentity (message.PackageId, version);
				return await packageManager.PreviewInstallPackageAsync (
					nugetProject,
					identity,
					resolutionContext,
					context,
					repositories,
					null,
					CancellationToken.None
				).ConfigureAwait (false);
			}
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

		static async Task<NuGetVersion> GetLatestPackageVersion (
			InstallPackageParams message,
			NuGetProject nugetProject,
			MonoDevelopNuGetPackageManager packageManager,
			IEnumerable<SourceRepository> repositories,
			NuGetProjectContext context,
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
				new ProjectContextLogger (context),
				CancellationToken.None);

			if (resolvedPackage?.LatestVersion == null) {
				throw new InvalidOperationException (GettextCatalog.GetString ("Unable to find package '{0}", message.PackageId));
			}

			return resolvedPackage?.LatestVersion;
		}

		[JsonRpcMethod (Methods.ProjectInstallPackage)]
		public void OnInstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<InstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				InstallPackageAsync (project, message).Wait ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnUninstallPackage error: {0}", ex);
				throw;
			}
		}

		async Task InstallPackageAsync (DotNetProject project, InstallPackageParams message)
		{
			var solutionManager = GetSolutionManager (project);
			var nugetProject = CreateNuGetProject (solutionManager, project);

			var packageManager = new MonoDevelopNuGetPackageManager (solutionManager);
			var repositories = GetSourceRepositories (message.PackageSources);

			var context = new NuGetProjectContext (solutionManager.Settings);
			var dependencyBehavior = (DependencyBehavior)Enum.Parse (typeof (DependencyBehavior), message.DependencyBehavior);

			using (var sourceCacheContext = new SourceCacheContext ()) {

				NuGetVersion version = null;
				if (string.IsNullOrEmpty (message.PackageVersion)) {
					version = await GetLatestPackageVersion (message, nugetProject, packageManager, repositories, context, dependencyBehavior, sourceCacheContext);
				} else {
					version = NuGetVersion.Parse (message.PackageVersion);
				}

				var resolutionContext = new ResolutionContext (
					dependencyBehavior,
					message.AllowPrerelease,
					true,
					VersionConstraints.None,
					new GatherCache (),
					sourceCacheContext
				);

				var identity = new PackageIdentity (message.PackageId, version);
				var actions = await packageManager.PreviewInstallPackageAsync (
					nugetProject,
					identity,
					resolutionContext,
					context,
					repositories,
					null,
					CancellationToken.None
				).ConfigureAwait (false);

				NuGetPackageManager.SetDirectInstall (identity, context);
				await packageManager.ExecuteNuGetProjectActionsAsync (
					nugetProject,
					actions,
					context,
					sourceCacheContext,
					CancellationToken.None);
				NuGetPackageManager.ClearDirectInstall (context);
			}
		}
	}
}
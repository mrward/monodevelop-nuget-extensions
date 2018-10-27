// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
// Based on code from VsConsole/PowerShellHost/PowerShellHost.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.PackageManagement;
using MonoDevelop.PackageManagement.Scripting;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.Protocol.Core.Types;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging;
using NuGet.Resolver;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet (VerbsLifecycle.Invoke, "InitializePackages", DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
	public class InvokeInitializePackagesCmdlet : PackageManagementCmdlet
	{
		public InvokeInitializePackagesCmdlet ()
			: this (
				PackageManagementExtendedServices.ConsoleHost,
				null)
		{
		}

		internal InvokeInitializePackagesCmdlet (
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base (consoleHost, terminatingError)
		{
		}

		protected override void ProcessRecord ()
		{
			if (!ConsoleHost.IsSolutionOpen)
				return;

			UpdateWorkingDirectory ();
			ExecuteInitScriptsAsync ().Wait ();
		}

		void UpdateWorkingDirectory ()
		{
			string command = "Invoke-UpdateWorkingDirectory";
			InvokeScript (command);
		}

		async Task ExecuteInitScriptsAsync ()
		{
			var projects = ConsoleHost.GetNuGetProjects ().ToList ();
			var packageManager = ConsoleHost.CreatePackageManager ();

			var packagesByFramework = new Dictionary<NuGetFramework, HashSet<PackageIdentity>> ();
			var sortedGlobalPackages = new List<PackageIdentity>();

			foreach (var project in projects) {
				var buildIntegratedProject = project as BuildIntegratedNuGetProject;

				if (buildIntegratedProject != null) {
					var packages = await BuildIntegratedProjectUtility
						.GetOrderedProjectPackageDependencies (buildIntegratedProject);

					sortedGlobalPackages.AddRange (packages);
				} else {
					// Read packages.config
					var installedRefs = await project.GetInstalledPackagesAsync (CancellationToken.None);

					if (installedRefs?.Any() == true) {
						// Index packages.config references by target framework since this affects dependencies
						NuGetFramework targetFramework;
						if (!project.TryGetMetadata (NuGetProjectMetadataKeys.TargetFramework, out targetFramework)) {
							targetFramework = NuGetFramework.AnyFramework;
						}

						HashSet<PackageIdentity> fwPackages;
						if (!packagesByFramework.TryGetValue (targetFramework, out fwPackages)) {
							fwPackages = new HashSet<PackageIdentity> ();
							packagesByFramework.Add(targetFramework, fwPackages);
						}

						fwPackages.UnionWith (installedRefs.Select (reference => reference.PackageIdentity));
					}
				}
			}

			// Each id/version should only be executed once
			var finishedPackages = new HashSet<PackageIdentity>();

			// Packages.config projects
			if (packagesByFramework.Count > 0) {
				await ExecuteInitPs1ForPackagesConfig (
					packageManager,
					packagesByFramework,
					finishedPackages);
			}

			// build integrated projects
			if (sortedGlobalPackages.Count > 0) {
				ExecuteInitPs1ForBuildIntegrated (
					sortedGlobalPackages,
					finishedPackages);
			}
		}

		async Task ExecuteInitPs1ForPackagesConfig (
			ConsoleHostNuGetPackageManager packageManager,
			Dictionary<NuGetFramework, HashSet<PackageIdentity>> packagesConfigInstalled,
			HashSet<PackageIdentity> finishedPackages)
		{
			// Get the path to the Packages folder.
			var packagesFolderPath = packageManager.PackageManager.PackagesFolderSourceRepository.PackageSource.Source;
			var packagePathResolver = new PackagePathResolver (packagesFolderPath);

			var packagesToSort = new HashSet<ResolverPackage> ();
			var resolvedPackages = new HashSet<PackageIdentity> ();

			var dependencyInfoResource = await packageManager
				.PackageManager
				.PackagesFolderSourceRepository
				.GetResourceAsync<DependencyInfoResource> ();

			using (var sourceCacheContext = new SourceCacheContext ()) {
				// Order by the highest framework first to make this deterministic
				// Process each framework/id/version once to avoid duplicate work
				// Packages may have different dependendcy orders depending on the framework, but there is
				// no way to fully solve this across an entire solution so we make a best effort here.
				foreach (var framework in packagesConfigInstalled.Keys.OrderByDescending (fw => fw, new NuGetFrameworkSorter ())) {
					foreach (var package in packagesConfigInstalled[framework]) {
						if (resolvedPackages.Add (package)) {
							var dependencyInfo = await dependencyInfoResource.ResolvePackage (
								package,
								framework,
								sourceCacheContext,
								NullLogger.Instance,
								CancellationToken.None);

							// This will be null for unrestored packages
							if (dependencyInfo != null) {
								packagesToSort.Add (new ResolverPackage (dependencyInfo, listed: true, absent: false));
							}
						}
					}
				}
			}

			// Order packages by dependency order
			var sortedPackages = ResolverUtility.TopologicalSort (packagesToSort);
			foreach (var package in sortedPackages) {
				if (finishedPackages.Add (package)) {
					// Find the package path in the packages folder.
					var installPath = packagePathResolver.GetInstalledPath (package);

					if (string.IsNullOrEmpty (installPath)) {
						continue;
					}

					ExecuteInitPs1 (installPath, package);
				}
			}
		}

		void ExecuteInitPs1ForBuildIntegrated (
			List<PackageIdentity> sortedGlobalPackages,
			HashSet<PackageIdentity> finishedPackages)
		{
			var nugetPaths = NuGetPathContext.Create (ConsoleHost.Settings);
			var fallbackResolver = new FallbackPackagePathResolver (nugetPaths);

			foreach (var package in sortedGlobalPackages) {
				if (finishedPackages.Add (package)) {
					// Find the package in the global packages folder or any of the fallback folders.
					var installPath = fallbackResolver.GetPackageDirectory (package.Id, package.Version);
					if (installPath == null) {
						continue;
					}

					ExecuteInitPs1 (installPath, package);
				}
			}
		}

		void ExecuteInitPs1 (string installPath, PackageIdentity identity)
		{
			try {
				var toolsPath = Path.Combine (installPath, "tools");
				if (Directory.Exists (toolsPath)) {
					AddPathToEnvironment (toolsPath);

					var scriptPath = Path.Combine (toolsPath, PowerShellScripts.Init);
					if (File.Exists (scriptPath) &&
						ConsoleHost.TryMarkInitScriptVisited (identity, PackageInitPS1State.FoundAndExecuted)) {

						var packageScript = new PackageScript (
							scriptPath,
							installPath,
							identity,
							null);

						var scriptRunner = (IPackageScriptRunner)this;
						scriptRunner.Run (packageScript);

						return;
					}
				}

				ConsoleHost.TryMarkInitScriptVisited (identity, PackageInitPS1State.NotFound);
			} catch (Exception ex) {
				// If execution of an init.ps1 scripts fails, do not let it crash our console.
				ReportError (ex.Message);
			}
		}

		static void AddPathToEnvironment (string path)
		{
			var currentPath = Environment.GetEnvironmentVariable ("PATH", EnvironmentVariableTarget.Process) ?? string.Empty;

			var currentPaths = new HashSet<string> (
				currentPath.Split (Path.PathSeparator).Select (p => p.Trim ()),
				StringComparer.OrdinalIgnoreCase);

			if (currentPaths.Add (path)) {
				var newPath = currentPath + Path.PathSeparator + path;
				Environment.SetEnvironmentVariable ("PATH", newPath, EnvironmentVariableTarget.Process);
			}
		}
	}
}

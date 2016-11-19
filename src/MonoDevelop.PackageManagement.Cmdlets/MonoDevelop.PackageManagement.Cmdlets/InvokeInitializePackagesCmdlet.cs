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
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging;

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

			// if A -> B, we invoke B's init.ps1 before A's.
			var sortedPackages = new List<PackageIdentity> ();

			var packagesFolderPackages = new HashSet<PackageIdentity> (PackageIdentity.Comparer);
			var globalPackages = new HashSet<PackageIdentity> (PackageIdentity.Comparer);

			foreach (var project in projects) {
				var buildIntegratedProject = project as BuildIntegratedNuGetProject;

				if (buildIntegratedProject != null) {
					var packages = BuildIntegratedProjectUtility.GetOrderedProjectDependencies (buildIntegratedProject);
					sortedPackages.AddRange (packages);
					globalPackages.UnionWith (packages);
				} else {
					var installedRefs = await project.GetInstalledPackagesAsync (CancellationToken.None);

					if (installedRefs != null && installedRefs.Any ()) {
						// This will be an empty list if packages have not been restored
						var installedPackages = await packageManager.PackageManager.GetInstalledPackagesInDependencyOrder (project, CancellationToken.None);
						sortedPackages.AddRange (installedPackages);
						packagesFolderPackages.UnionWith (installedPackages);
					}
				}
			}

			// Get the path to the Packages folder.
			var packagesFolderPath = packageManager.PackageManager.PackagesFolderSourceRepository.PackageSource.Source;
			var packagePathResolver = new PackagePathResolver (packagesFolderPath);

			var globalFolderPath = SettingsUtility.GetGlobalPackagesFolder (ConsoleHost.SolutionManager.Settings);
			var globalPathResolver = new VersionFolderPathResolver (globalFolderPath);

			var finishedPackages = new HashSet<PackageIdentity> (PackageIdentity.Comparer);

			foreach (var package in sortedPackages) {
				// Packages may occur under multiple projects, but we only need to run it once.
				if (!finishedPackages.Contains (package)) {
					finishedPackages.Add (package);

					try {
						string pathToPackage = null;

						// If the package exists in both the global and packages folder, use the packages folder copy.
						if (packagesFolderPackages.Contains (package)) {
							// Local package in the packages folder
							pathToPackage = packagePathResolver.GetInstalledPath (package);
						} else {
							// Global package
							pathToPackage = globalPathResolver.GetInstallPath (package.Id, package.Version);
						}

						if (!string.IsNullOrEmpty(pathToPackage)) {
							var toolsPath = Path.Combine (pathToPackage, "tools");
							var scriptPath = Path.Combine (toolsPath, PowerShellScripts.Init);

							if (Directory.Exists (toolsPath)) {
								AddPathToEnvironment (toolsPath);
								if (File.Exists (scriptPath)) {
									if (ConsoleHost.TryMarkInitScriptVisited (
										package,
										PackageInitPS1State.FoundAndExecuted)) {

										var packageScript = new PackageScript (
											scriptPath,
											pathToPackage,
											package,
											null);

										var scriptRunner = (IPackageScriptRunner)this;
										scriptRunner.Run (packageScript);
									}
								} else {
									ConsoleHost.TryMarkInitScriptVisited (package, PackageInitPS1State.NotFound);
								}
							}
						}
					} catch (Exception ex) {
						// if execution of Init scripts fails, do not let it crash our console
						ReportError (ex.Message);
					}
				}
			}
		}

		static void AddPathToEnvironment (string path)
		{
			if (Directory.Exists (path)) {
				string environmentPath = Environment.GetEnvironmentVariable ("path", EnvironmentVariableTarget.Process);
				environmentPath = environmentPath + ";" + path;
				Environment.SetEnvironmentVariable ("path", environmentPath, EnvironmentVariableTarget.Process);
			}
		}
	}
}

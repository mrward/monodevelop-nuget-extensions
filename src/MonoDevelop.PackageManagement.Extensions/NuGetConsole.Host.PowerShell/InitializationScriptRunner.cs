// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
// Based on src/NuGet.Clients/NuGetConsole.Host.PowerShell/PowerShellHost.cs

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement;
using MonoDevelop.Projects;
using NuGet.PackageManagement;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using MonoDevelop.PackageManagement.Protocol;

namespace NuGetConsole.Host.PowerShell
{
	class InitializationScriptRunner
	{
		readonly Solution solution;
		readonly IScriptExecutor scriptExecutor;

		public InitializationScriptRunner (
			Solution solution,
			IScriptExecutor scriptExecutor)
		{
			this.solution = solution;
			this.scriptExecutor = scriptExecutor;
		}

		public async Task ExecuteInitScriptsAsync (CancellationToken token)
		{
			var solutionManager = GetSolutionManager (solution);
			var repositoryProvider = solutionManager.CreateSourceRepositoryProvider ();

			var packageManager = new NuGetPackageManager (
				solutionManager.CreateSourceRepositoryProvider (),
				solutionManager.Settings,
				solutionManager,
				new DeleteOnRestartManager ());

			var enumerator = new InstalledPackageEnumerator (solutionManager, solutionManager.Settings);
			var installedPackages = await enumerator.EnumeratePackagesAsync (packageManager, token);

			var context = new ConsoleHostNuGetProjectContext (solutionManager.Settings);
			context.IsExecutingPowerShellCommand = false;

			foreach (var installedPackage in installedPackages) {
				await ExecuteInitPs1Async (installedPackage.InstallPath, installedPackage.Identity, context, token);
			}
		}

		async Task ExecuteInitPs1Async (
			string installPath,
			PackageIdentity identity,
			INuGetProjectContext context,
			CancellationToken token)
		{
			try {
				var toolsPath = Path.Combine (installPath, "tools");
				if (Directory.Exists (toolsPath)) {
					//AddPathToEnvironment (toolsPath);

					var relativePath = Path.Combine ("tools", PowerShellScripts.Init);

					await scriptExecutor.ExecuteAsync (
						identity,
						installPath,
						relativePath,
						null,
						context,
						false,
						token);

					return;
				}

				scriptExecutor.TryMarkVisited (identity, PackageInitPS1State.NotFound);
			} catch (Exception ex) {
				LoggingService.LogError ("ExecuteInitPs1Async error", ex);
			}
		}

		//static void AddPathToEnvironment (string path)
		//{
		//	var currentPath = Environment.GetEnvironmentVariable ("path", EnvironmentVariableTarget.Process);

		//	var currentPaths = new HashSet<string> (
		//		currentPath.Split (Path.PathSeparator).Select (p => p.Trim ()),
		//		StringComparer.OrdinalIgnoreCase);

		//	if (currentPaths.Add (path)) {
		//		var newPath = currentPath + Path.PathSeparator + path;
		//		Environment.SetEnvironmentVariable ("path", newPath, EnvironmentVariableTarget.Process);
		//	}
		//}

		static IMonoDevelopSolutionManager GetSolutionManager (Solution solution)
		{
			return Runtime.RunInMainThread (() => {
				return PackageManagementServices.Workspace.GetSolutionManager (solution);
			}).WaitAndGetResult ();
		}
	}
}

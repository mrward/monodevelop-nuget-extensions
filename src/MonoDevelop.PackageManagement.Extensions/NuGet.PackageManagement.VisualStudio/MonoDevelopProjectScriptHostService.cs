// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.PackageManagement;
using MonoDevelop.Projects;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;

namespace NuGet.PackageManagement.VisualStudio
{
	internal class MonoDevelopProjectScriptHostService : IProjectScriptHostService
	{
		readonly DotNetProject project;
		Lazy<IScriptExecutor> lazyScriptExecutor;

		public MonoDevelopProjectScriptHostService (
			DotNetProject project,
			INuGetProjectServices projectServices)
		{
			this.project = project;

			lazyScriptExecutor = new Lazy<IScriptExecutor> (
				() => PackageManagementExtendedServices.ConsoleHost.ScriptExecutor);
		}

		public Task ExecutePackageScriptAsync (
			PackageIdentity packageIdentity,
			string packageInstallPath,
			string scriptRelativePath,
			INuGetProjectContext projectContext,
			bool throwOnFailure,
			CancellationToken token)
		{
			var scriptExecutor = lazyScriptExecutor.Value;

			return scriptExecutor.ExecuteAsync (
				packageIdentity,
				packageInstallPath,
				scriptRelativePath,
				project,
				projectContext,
				throwOnFailure);
		}

		public async Task<bool> ExecutePackageInitScriptAsync (
			PackageIdentity packageIdentity,
			string packageInstallPath,
			INuGetProjectContext projectContext,
			bool throwOnFailure,
			CancellationToken token)
		{
			var scriptExecutor = lazyScriptExecutor.Value;

			using (var packageReader = new PackageFolderReader (packageInstallPath)) {
				var toolItemGroups = packageReader.GetToolItems ();

				if (toolItemGroups != null) {
					// Init.ps1 must be found at the root folder, target frameworks are not recognized here,
					// since this is run for the solution.
					var toolItemGroup = toolItemGroups
						.FirstOrDefault (group => group.TargetFramework.IsAny);

					if (toolItemGroup != null) {
						var initPS1RelativePath = toolItemGroup
							.Items
							.FirstOrDefault (p => p.StartsWith (
								 PowerShellScripts.InitPS1RelativePath,
								 StringComparison.OrdinalIgnoreCase));

						if (!string.IsNullOrEmpty (initPS1RelativePath)) {
							initPS1RelativePath = NuGet.Common.PathUtility.ReplaceAltDirSeparatorWithDirSeparator (
								initPS1RelativePath);

							return await scriptExecutor.ExecuteAsync (
								packageIdentity,
								packageInstallPath,
								initPS1RelativePath,
								project,
								projectContext,
								throwOnFailure);
						}
					}
				}
			}

			return false;
		}
	}
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.PackageManagement.Scripting;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;

namespace MonoDevelop.PackageManagement
{
	class ConsoleHostScriptRunner
	{
		ConcurrentDictionary<PackageIdentity, PackageInitPS1State> InitScriptExecutions
			= new ConcurrentDictionary<PackageIdentity, PackageInitPS1State> (PackageIdentityComparer.Default);

		public void Reset ()
		{
			InitScriptExecutions.Clear ();
		}

		public Task ExecuteScriptAsync (
			PackageIdentity identity,
			string packageInstallPath,
			string scriptRelativePath,
			IDotNetProject project,
			INuGetProjectContext nuGetProjectContext,
			bool throwOnFailure)
		{
			string scriptPath = Path.Combine (packageInstallPath, scriptRelativePath);

			if (!File.Exists (scriptPath)) {
				if (IsInitScript (scriptPath)) {
					TryMarkVisited (identity, PackageInitPS1State.NotFound);
				}
				return Task.FromResult (0);
			}

			if (IsInitScript (scriptPath) &&
				!TryMarkVisited (identity, PackageInitPS1State.FoundAndExecuted)) {
				return Task.FromResult (0);
			}

			var packageScript = new PackageScript (
				scriptPath,
				packageInstallPath,
				identity,
				project);

			var scriptRunner = nuGetProjectContext as IPackageScriptRunner;
			if (scriptRunner != null) {
				scriptRunner.Run (packageScript);
			}

			return Task.FromResult (0);
		}

		static bool IsInitScript (string scriptPath)
		{
			return scriptPath.EndsWith (PowerShellScripts.Init, StringComparison.OrdinalIgnoreCase);
		}

		public bool TryMarkVisited (PackageIdentity packageIdentity, PackageInitPS1State initPS1State)
		{
			return InitScriptExecutions.TryAdd (packageIdentity, initPS1State);
		}
	}
}

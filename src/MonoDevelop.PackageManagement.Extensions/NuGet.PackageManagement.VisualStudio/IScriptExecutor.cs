// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Projects;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;

namespace NuGet.PackageManagement.VisualStudio
{
	interface IScriptExecutor
	{
		void Reset ();

		Task<bool> ExecuteAsync (
			PackageIdentity packageIdentity,
			string packageInstallPath,
			string scriptRelativePath,
			DotNetProject project,
			INuGetProjectContext nuGetProjectContext,
			bool throwOnFailure);

		bool TryMarkVisited (PackageIdentity packageIdentity, PackageInitPS1State initPS1State);

		Task<bool> ExecuteInitScriptAsync (PackageIdentity packageIdentity, CancellationToken token);
	}
}
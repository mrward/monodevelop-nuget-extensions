//
// IConsoleHostNuGetPackageManager.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2022 Microsoft
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;

namespace MonoDevelop.PackageManagement
{
	public interface IConsoleHostNuGetPackageManager
	{
		void ClearDirectInstall (INuGetProjectContext nuGetProjectContext);
		void SetDirectInstall (PackageIdentity directInstall, INuGetProjectContext nuGetProjectContext);

		Task<IEnumerable<NuGetProjectAction>> PreviewInstallPackageAsync (
			NuGetProject nuGetProject,
			PackageIdentity packageIdentity,
			ResolutionContext resolutionContext,
			INuGetProjectContext nuGetProjectContext,
			IEnumerable<SourceRepository> primarySources,
			IEnumerable<SourceRepository> secondarySources,
			CancellationToken token);

		Task<IEnumerable<NuGetProjectAction>> PreviewInstallPackageAsync (
			NuGetProject nuGetProject,
			string packageIdentity,
			ResolutionContext resolutionContext,
			INuGetProjectContext nuGetProjectContext,
			IEnumerable<SourceRepository> primarySources,
			IEnumerable<SourceRepository> secondarySources,
			CancellationToken token);

		Task<IEnumerable<NuGetProjectAction>> PreviewUninstallPackageAsync (
			NuGetProject nuGetProject,
			string packageId,
			UninstallationContext uninstallationContext,
			INuGetProjectContext nuGetProjectContext,
			CancellationToken token);

		Task<IEnumerable<NuGetProjectAction>> PreviewUpdatePackagesAsync (
			string packageId,
			IEnumerable<NuGetProject> nuGetProjects,
			ResolutionContext resolutionContext,
			INuGetProjectContext nuGetProjectContext,
			IEnumerable<SourceRepository> primarySources,
			IEnumerable<SourceRepository> secondarySources,
			CancellationToken token);

		Task<IEnumerable<NuGetProjectAction>> PreviewUpdatePackagesAsync (
			PackageIdentity packageIdentity,
			IEnumerable<NuGetProject> nuGetProjects,
			ResolutionContext resolutionContext,
			INuGetProjectContext nuGetProjectContext,
			IEnumerable<SourceRepository> primarySources,
			IEnumerable<SourceRepository> secondarySources,
			CancellationToken token);

		Task<IEnumerable<NuGetProjectAction>> PreviewUpdatePackagesAsync (
			IEnumerable<NuGetProject> nuGetProjects,
			ResolutionContext resolutionContext,
			INuGetProjectContext nuGetProjectContext,
			IEnumerable<SourceRepository> primarySources,
			IEnumerable<SourceRepository> secondarySources,
			CancellationToken token);

		Task ExecuteNuGetProjectActionsAsync (
			IEnumerable<NuGetProject> nuGetProjects,
			IEnumerable<NuGetProjectAction> nuGetProjectActions,
			INuGetProjectContext nuGetProjectContext,
			SourceCacheContext sourceCacheContext,
			CancellationToken token);

		Task ExecuteNuGetProjectActionsAsync (
			NuGetProject nuGetProject,
			IEnumerable<NuGetProjectAction> nuGetProjectActions,
			INuGetProjectContext nuGetProjectContext,
			SourceCacheContext sourceCacheContext,
			CancellationToken token);
	}
}


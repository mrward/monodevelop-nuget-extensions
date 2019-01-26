//
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
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	public class ProjectMessageHandler
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
	}
}

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
using NuGet.PackageManagement;
using NuGet.Packaging;
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
				var packages = GetInstalledPackages (project).WaitAndGetResult ();
				return new ProjectPackagesList {
					Packages = CreatePackageInformation (packages).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetInstalledPackages error", ex);
				throw;
			}
		}

		async Task<IEnumerable<PackageReference>> GetInstalledPackages (DotNetProject project)
		{
			var solutionManager = project.GetSolutionManager ();
			var nugetProject =  project.CreateNuGetProject (solutionManager);

			return await Task.Run (() => nugetProject.GetInstalledPackagesAsync (CancellationToken.None)).ConfigureAwait (false);
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
				var handler = new UninstallPackageMessageHandler (project, message);
				var actions = handler.PreviewUninstallPackage (CancellationToken.None).WaitAndGetResult ();
				return new PackageActionList {
					Actions = CreatePackageActionInformation (actions).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnPreviewUninstallPackage error", ex);
				throw;
			}
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
				var handler = new UninstallPackageMessageHandler (project, message);
				handler.UninstallPackageAsync (CancellationToken.None).WaitAndGetResult ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnUninstallPackage error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectPreviewInstallPackage)]
		public PackageActionList OnPreviewInstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<InstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				var handler = new InstallPackageMessageHandler (project, message);
				var actions = handler.PreviewInstallPackage (CancellationToken.None).WaitAndGetResult ();
				return new PackageActionList {
					Actions = CreatePackageActionInformation (actions).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnPreviewInstallPackage error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectInstallPackage)]
		public void OnInstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<InstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				var handler = new InstallPackageMessageHandler (project, message);
				handler.InstallPackageAsync (CancellationToken.None).WaitAndGetResult ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnInstallPackage error", ex);
				throw;
			}
		}
	}
}

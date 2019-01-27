//
// ProjectExtensions.cs
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
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core
{
	public static class ProjectExtensions
	{
		public static async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync (
			this Project project,
			CancellationToken token)
		{
			var message = new ProjectParams {
				FileName = project.FileName
			};
			var list = await JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<ProjectPackagesList> (
				Methods.ProjectInstalledPackagesName,
				message,
				token);
			return ToPackageReferences (list.Packages);
		}

		static IEnumerable<PackageReference> ToPackageReferences (IEnumerable<PackageReferenceInfo> packages)
		{
			return packages.Select (package => CreatePackageReference (package));
		}

		static PackageReference CreatePackageReference (PackageReferenceInfo package)
		{
			return new PackageReference (
				new PackageIdentity (package.Id, new NuGetVersion (package.Version)),
				NuGetFramework.Parse (package.TargetFramework),
				package.IsUserInstalled,
				package.IsDevelopmentDependency,
				package.RequireReinstallation,
				GetVersionRange (package)
			);
		}

		static VersionRange GetVersionRange (PackageReferenceInfo package)
		{
			if (string.IsNullOrEmpty (package.VersionRange)) {
				return null;
			}

			return VersionRange.Parse (package.VersionRange);
		}

		public static async Task<IEnumerable<PackageActionInfo>> PreviewUninstallPackageAsync (
			this Project project,
			string packageId,
			UninstallationContext uninstallContext,
			CancellationToken token)
		{
			var message = new UninstallPackageParams {
				ProjectFileName = project.FileName,
				PackageId = packageId,
				Force = uninstallContext.ForceRemove,
				RemoveDependencies = uninstallContext.RemoveDependencies
			};
			var list = await JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<PackageActionList> (
				Methods.ProjectPreviewUninstallPackage,
				message,
				token);
			return list.Actions;
		}

		public static async Task UninstallPackageAsync (
			this Project project,
			string packageId,
			UninstallationContext uninstallContext,
			CancellationToken token)
		{
			var message = new UninstallPackageParams {
				ProjectFileName = project.FileName,
				PackageId = packageId,
				Force = uninstallContext.ForceRemove,
				RemoveDependencies = uninstallContext.RemoveDependencies
			};
			await JsonRpcProvider.Rpc.InvokeWithCancellationAsync (
				Methods.ProjectUninstallPackage,
				new [] { message },
				token);
		}

		public static Task<IEnumerable<PackageActionInfo>> PreviewInstallPackageAsync (
			this Project project,
			string packageId,
			DependencyBehavior dependencyBehaviour,
			bool allowPrerelease,
			IEnumerable<SourceRepository> sources,
			CancellationToken token)
		{
			return PreviewInstallPackageAsync (project, packageId, null, dependencyBehaviour, allowPrerelease, sources, token);
		}

		public static async Task<IEnumerable<PackageActionInfo>> PreviewInstallPackageAsync (
			this Project project,
			string packageId,
			string packageVersion,
			DependencyBehavior dependencyBehaviour,
			bool allowPrerelease,
			IEnumerable<SourceRepository> sources,
			CancellationToken token)
		{
			var message = new InstallPackageParams {
				ProjectFileName = project.FileName,
				PackageId = packageId,
				PackageVersion = packageVersion,
				DependencyBehavior = dependencyBehaviour.ToString (),
				AllowPrerelease = allowPrerelease,
				PackageSources = GetPackageSourceInfo (sources).ToArray ()
			};
			var list = await JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<PackageActionList> (
				Methods.ProjectPreviewInstallPackage,
				message,
				token);
			return list.Actions;
		}

		static IEnumerable<PackageSourceInfo> GetPackageSourceInfo (IEnumerable<SourceRepository> sources)
		{
			return sources.Select (source => GetPackageSourceInfo (source));
		}

		static PackageSourceInfo GetPackageSourceInfo (SourceRepository source)
		{
			return new PackageSourceInfo {
				Name = source.PackageSource.Name,
				Source = source.PackageSource.Source
			};
		}
	}
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.PackageManagement;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.VisualStudio;
using Task = System.Threading.Tasks.Task;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	/// <summary>
	/// This command consolidates the specified package into the specified project.
	/// </summary>
	[Cmdlet (VerbsData.Sync, "Package")]
	public class SyncPackageCommand : PackageActionBaseCommand
	{
		bool allowPrerelease;

		List<NuGetProject> Projects = new List<NuGetProject> ();

		protected override void Preprocess ()
		{
			base.Preprocess ();
			if (string.IsNullOrEmpty (ProjectName)) {
				ProjectName = Project.GetName ();
			}

			NuGetUIThreadHelper.JoinableTaskFactory.Run (async () => {
				// Get the projects in the solution that's not the current default or specified project to sync the package identity to.
				var projects = await SolutionManager.GetAllNuGetProjectsAsync ();
				Projects = projects
					.Where (p => !StringComparer.OrdinalIgnoreCase.Equals (p.GetName (), ProjectName))
					.ToList ();
			});
		}

		protected override void ProcessRecordCore ()
		{
			Preprocess ();

			var identity = NuGetUIThreadHelper.JoinableTaskFactory.Run (async delegate {
				var result = await GetPackageIdentity ();
				return result;
			});

			if (Projects.Count == 0) {
				LogCore (MessageLevel.Info, string.Format (
					CultureInfo.CurrentCulture,
					"There are no other projects to sync the package '{0}'.",
					Id));
			} else if (identity == null) {
				LogCore (MessageLevel.Info, string.Format (
					CultureInfo.CurrentCulture,
					"Package with the Id '{0}' is not installed in project '{1}'.",
					Id,
					ProjectName));
			} else {
				allowPrerelease = IncludePrerelease.IsPresent || identity.Version.IsPrerelease;
				Task.Run (() => SyncPackages (Projects, identity));
				WaitAndLogPackageActions ();
			}
		}

		/// <summary>
		/// Async call for sync package to the version installed to the specified or current project.
		/// </summary>
		async Task SyncPackages (IEnumerable<NuGetProject> projects, PackageIdentity identity)
		{
			try {
				using (var sourceCacheContext = new SourceCacheContext ()) {
					var resolutionContext = new ResolutionContext (
						GetDependencyBehavior (),
						allowPrerelease,
						false,
						VersionConstraints.None,
						new GatherCache (),
						sourceCacheContext);

					foreach (var project in projects) {
						await InstallPackageByIdentityAsync (project, identity, resolutionContext, this, WhatIf.IsPresent);
					}
				}
			} catch (Exception ex) {
				Log (MessageLevel.Error, ExceptionUtilities.DisplayMessage (ex));
			} finally {
				BlockingCollection.Add (new ExecutionCompleteMessage ());
			}
		}

		/// <summary>
		/// Returns single package identity for resolver when Id is specified
		/// </summary>
		async Task<PackageIdentity> GetPackageIdentity ()
		{
			PackageIdentity identity = null;
			if (!string.IsNullOrEmpty (Version)) {
				var nVersion = PowerShellCmdletsUtility.GetNuGetVersionFromString (Version);
				identity = new PackageIdentity (Id, nVersion);
			} else {
				identity = (await Project.GetInstalledPackagesAsync (CancellationToken.None))
					.Where (p => string.Equals (p.PackageIdentity.Id, Id, StringComparison.OrdinalIgnoreCase))
					.Select (v => v.PackageIdentity).FirstOrDefault ();
			}
			return identity;
		}
	}
}
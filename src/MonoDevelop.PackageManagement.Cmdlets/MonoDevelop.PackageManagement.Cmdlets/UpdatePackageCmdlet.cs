// 
// UpdatePackageCmdlet.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Management.Automation;
using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement;
using MonoDevelop.PackageManagement.Scripting;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.PackageManagement.PowerShellCmdlets;
using NuGet.Resolver;
using NuGetVersion = NuGet.Versioning.NuGetVersion;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet (VerbsData.Update, "Package", DefaultParameterSetName = "All")]
	public class UpdatePackageCmdlet : PackageActionBaseCmdlet
	{
		string id;
		string projectName;
		bool idSpecified;
		bool projectSpecified;
		bool versionSpecifiedPrerelease;
		bool allowPrerelease;
		bool isPackageInstalled;
		NuGetVersion nugetVersion;
		List<NuGetProject> projects;

		public UpdatePackageCmdlet ()
			: this (
				PackageManagementExtendedServices.ConsoleHost,
				null)
		{
		}

		internal UpdatePackageCmdlet (
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base (consoleHost, terminatingError)
		{
		}

		[Parameter (Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, ParameterSetName = "Project")]
		[Parameter (ValueFromPipelineByPropertyName = true, Position = 0, ParameterSetName = "All")]
		[Parameter (ValueFromPipelineByPropertyName = true, Position = 0, ParameterSetName = "Reinstall")]
		public override string Id
		{
			get { return id; }
			set
			{
				id = value;
				idSpecified = true;
			}
		}

		[Parameter (Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "All")]
		[Parameter (Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "Project")]
		[Parameter (Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "Reinstall")]
		public override string ProjectName
		{
			get { return projectName; }
			set
			{
				projectName = value;
				projectSpecified = true;
			}
		}

		[Parameter (Position = 2, ParameterSetName = "Project")]
		[ValidateNotNullOrEmpty]
		public override string Version { get; set; }

		[Parameter]
		[Alias ("ToHighestPatch")]
		public SwitchParameter Safe { get; set; }

		[Parameter]
		public SwitchParameter ToHighestMinor { get; set; }

		[Parameter (Mandatory = true, ParameterSetName = "Reinstall")]
		[Parameter (ParameterSetName = "All")]
		public SwitchParameter Reinstall { get; set; }

		protected virtual void Preprocess ()
		{
			CheckSolutionIsOpen ();
			UpdateActiveSourceRepository (Source);
			DetermineFileConflictAction ();

			ParseUserInputForVersion ();

			if (!projectSpecified) {
				projects = ConsoleHost.GetNuGetProjects ().ToList ();
			} else {
				NuGetProject project = ConsoleHost.GetNuGetProject (ProjectName);
				if (project == null)
					ThrowProjectNotOpenTerminatingError ();

				projects = new List<NuGetProject> { project };
			}

			if (Reinstall) {
				ActionType = NuGetActionType.Reinstall;
			} else {
				ActionType = NuGetActionType.Update;
			}
		}

		protected override void ProcessRecord ()
		{
			Preprocess ();
			using (IConsoleHostFileConflictResolver resolver = CreateFileConflictResolver ()) {
				using (IDisposable monitor = CreateEventsMonitor ()) {
					RunUpdate ().Wait ();
				}
			}
		}

		IConsoleHostFileConflictResolver CreateFileConflictResolver ()
		{
			return ConsoleHost.CreateFileConflictResolver (FileConflictAction);
		}

		Task RunUpdate ()
		{
			if (!idSpecified) {
				return UpdateOrReinstallAllPackages ();
			} else {
				return UpdateOrReinstallSinglePackage ();
			}
		}

		void ParseUserInputForVersion ()
		{
			if (!string.IsNullOrEmpty (Version)) {
				// If Version is prerelease, automatically allow prerelease (i.e. append -Prerelease switch).
				nugetVersion = PowerShellCmdletsUtility.GetNuGetVersionFromString (Version);
				if (nugetVersion.IsPrerelease) {
					versionSpecifiedPrerelease = true;
				}
			}
			allowPrerelease = IncludePrerelease.IsPresent || versionSpecifiedPrerelease;
		}

		/// <summary>
		/// Update or reinstall all packages installed to a solution. For Update-Package or Update-Package -Reinstall.
		/// </summary>
		async Task UpdateOrReinstallAllPackages ()
		{
			foreach (var project in projects) {
				await PreviewAndExecuteUpdateActionsforAllPackages (project);
			}
		}

		/// <summary>
		/// Update or reinstall a single package installed to a solution. For Update-Package -Id or 
		/// Update-Package -Id -Reinstall.
		/// </summary>
		async Task UpdateOrReinstallSinglePackage ()
		{
			foreach (var project in projects) {
				await PreviewAndExecuteUpdateActionsforSinglePackage (project);
			}

			if (!isPackageInstalled) {
				Log (MessageLevel.Error, GettextCatalog.GetString ("'{0}' was not installed in any project. Update failed.", Id));
			}
		}

		async Task PreviewAndExecuteUpdateActionsforAllPackages (NuGetProject project)
		{
			var packageManager = ConsoleHost.CreatePackageManager ();
			var actions = await packageManager.PreviewUpdatePackagesAsync (
				project,
				CreateResolutionContext (),
				this,
				PrimarySourceRepositories,
				PrimarySourceRepositories,
				ConsoleHost.Token);

			await ExecuteActions (project, actions, packageManager);
		}

		async Task PreviewAndExecuteUpdateActionsforSinglePackage (NuGetProject project)
		{
			ConsoleHostNuGetPackageManager packageManager = null;

			var installedPackage = (await project.GetInstalledPackagesAsync (ConsoleHost.Token))
				.FirstOrDefault (p => string.Equals (p.PackageIdentity.Id, Id, StringComparison.OrdinalIgnoreCase));

			if (installedPackage != null) {
				// set _installed to true, if package to update is installed.
				isPackageInstalled = true;

				packageManager = ConsoleHost.CreatePackageManager ();
				var actions = Enumerable.Empty<NuGetProjectAction> ();

				// If -Version switch is specified
				if (!string.IsNullOrEmpty (Version)) {
					actions = await packageManager.PreviewUpdatePackagesAsync (
						new PackageIdentity (installedPackage.PackageIdentity.Id, PowerShellCmdletsUtility.GetNuGetVersionFromString (Version)),
						project,
						CreateResolutionContext (),
						this,
						PrimarySourceRepositories,
						EnabledSourceRepositories,
						ConsoleHost.Token);
				} else {
					actions = await packageManager.PreviewUpdatePackagesAsync (
						installedPackage.PackageIdentity.Id,
						project,
						CreateResolutionContext (),
						this,
						PrimarySourceRepositories,
						EnabledSourceRepositories,
						ConsoleHost.Token);
				}

				await ExecuteActions (project, actions, packageManager);
			}
		}

		async Task ExecuteActions (NuGetProject project, IEnumerable<NuGetProjectAction> actions, ConsoleHostNuGetPackageManager packageManager)
		{
			if (actions.Any ()) {
				if (WhatIf.IsPresent) {
					// For -WhatIf, only preview the actions
					PreviewNuGetPackageActions (actions);
				} else {
					// Execute project actions by Package Manager
					await packageManager.ExecuteNuGetProjectActionsAsync (project, actions, this, ConsoleHost.Token);
				}
			} else {
				Log (MessageLevel.Info,
					GettextCatalog.GetString ("No package updates are available from the current package source for project '{0}'.",
						project.GetMetadata<string> (NuGetProjectMetadataKeys.Name)));
			}
		}

		ResolutionContext CreateResolutionContext ()
		{
			return new ResolutionContext (GetDependencyBehavior (), allowPrerelease, false, DetermineVersionConstraints ());
		}

		protected override DependencyBehavior GetDependencyBehavior()
		{
			if (!idSpecified && !Reinstall.IsPresent) {
				return DependencyBehavior.Highest;
			}

			return base.GetDependencyBehavior ();
		}

		VersionConstraints DetermineVersionConstraints()
		{
			if (Reinstall.IsPresent) {
				return VersionConstraints.ExactMajor | VersionConstraints.ExactMinor | VersionConstraints.ExactPatch | VersionConstraints.ExactRelease;
			} else if (Safe.IsPresent) {
				return VersionConstraints.ExactMajor | VersionConstraints.ExactMinor;
			} else if (ToHighestMinor.IsPresent) {
				return VersionConstraints.ExactMajor;
			} else {
				return VersionConstraints.None;
			}
		}
	}
}

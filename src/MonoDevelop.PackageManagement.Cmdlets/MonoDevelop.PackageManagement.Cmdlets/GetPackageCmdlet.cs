// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.PackageManagement;
using NuGet.PackageManagement.PowerShellCmdlets;
using NuGet.ProjectManagement;
using MonoDevelop.Core;
using NuGet.Protocol.Core.Types;

using System.Threading.Tasks;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet (VerbsCommon.Get, "Package", DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
	public class GetPackageCmdlet : PackageManagementCmdlet
	{
		const int DefaultFirstValue = 50;

		List<NuGetProject> projects;
		bool collapseVersions;
		bool useRemoteSourceOnly;
		bool useRemoteSource;
		bool enablePaging;

		public GetPackageCmdlet ()
			: this (
				PackageManagementExtendedServices.ConsoleHost,
				null)
		{
		}

		internal GetPackageCmdlet (
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base (consoleHost, terminatingError)
		{
		}

		[Parameter (Position = 0)]
		[ValidateNotNullOrEmpty]
		public string Filter { get; set; }

		//[Parameter (Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Project")]
		[Parameter (Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "Project")]
		[ValidateNotNullOrEmpty]
		public string ProjectName { get; set; }

		[Parameter (ParameterSetName = "Remote")]
		[Parameter (ParameterSetName = "Updates")]
		[ValidateNotNullOrEmpty]
		public string Source { get; set; }

		[Parameter (Mandatory = true, ParameterSetName = "Remote")]
		[Alias ("Online", "Remote")]
		public SwitchParameter ListAvailable { get; set; }

		[Parameter (Mandatory = true, ParameterSetName = "Updates")]
		public SwitchParameter Updates { get; set; }

		[Parameter (ParameterSetName = "Remote")]
		[Parameter (ParameterSetName = "Updates")]
		public SwitchParameter AllVersions { get; set; }

		[Parameter (ParameterSetName = "Remote")]
		[ValidateRange (0, Int32.MaxValue)]
		public int PageSize { get; set; }

		[Parameter]
		[Alias ("Prerelease")]
		public SwitchParameter IncludePrerelease { get; set; }

		[Parameter]
		[ValidateRange (0, Int32.MaxValue)]
		public int First { get; set; }

		[Parameter]
		[ValidateRange(0, Int32.MaxValue)]
		public int Skip { get; set; }

		protected override void ProcessRecord ()
		{
			useRemoteSourceOnly = ListAvailable.IsPresent || (!String.IsNullOrEmpty (Source) && !Updates.IsPresent);
			useRemoteSource = ListAvailable.IsPresent || Updates.IsPresent || !String.IsNullOrEmpty (Source);
			collapseVersions = !AllVersions.IsPresent;

			projects = GetNuGetProjects ();

			if (useRemoteSource) {
				WriteRemoteSourcePackages ();
			} else {
				WriteInstalledPackages ();
			}
		}

		List<NuGetProject> GetNuGetProjects ()
		{
			if (HasSelectedProjectName ())
				return new List<NuGetProject> { ConsoleHost.GetNuGetProject (ProjectName) };

			return ConsoleHost.GetNuGetProjects ().ToList ();
		}

		bool HasSelectedProjectName ()
		{
			return ProjectName != null;
		}

		void WriteInstalledPackages ()
		{
			CheckSolutionIsOpen ();

			var packagesToDisplay = GetInstalledPackagesAsync (projects, Filter, Skip, First, ConsoleHost.Token);
			WriteInstalledPackages (packagesToDisplay.Result);
		}

		void WriteInstalledPackages (Dictionary<NuGetProject, IEnumerable<NuGet.Packaging.PackageReference>> packages)
		{
			List<PowerShellInstalledPackage> view = PowerShellInstalledPackage.GetPowerShellPackageView (packages, ConsoleHost.SolutionManager, ConsoleHost.Settings);
			if (view.Any ()) {
				WritePackagesToOutputPipeline (view);
			} else {
				Log (MessageLevel.Info, GettextCatalog.GetString ("No packages installed."));
			}
		}

		protected virtual void CmdletThrowTerminatingError (ErrorRecord errorRecord)
		{
			ThrowTerminatingError (errorRecord);
		}

		void WriteRemoteSourcePackages ()
		{
			UpdateActiveSourceRepository (Source);

			if (PageSize != 0) {
				enablePaging = true;
				First = PageSize;
			} else if (First == 0) {
				First = DefaultFirstValue;
			}

			if (Filter == null) {
				Filter = string.Empty;
			}

			if (useRemoteSourceOnly) {
				// Find available packages from the current source and not taking target frameworks into account.
				var remotePackages = GetPackagesFromRemoteSource (Filter, IncludePrerelease.IsPresent)
					.Skip(Skip);

				WritePackagesFromRemoteSource (remotePackages.Take (First), outputWarning: true);

				if (enablePaging) {
				//	WriteMoreRemotePackagesWithPaging (remotePackages.Skip (First));
				}
			} else {
				// Get package updates from the current source and taking target frameworks into account.
				CheckSolutionIsOpen ();
				WriteUpdatePackagesFromRemoteSourceAsyncInSolutionAsync ().Wait ();
			}
		}

		void WritePackagesToOutputPipeline (IEnumerable<PowerShellPackage> packages)
		{
			foreach (PowerShellPackage package in packages) {
				WriteObject (package);
			}
		}

		/// <summary>
		/// Output packages found from the current remote source
		/// </summary>
		void WritePackagesFromRemoteSource (IEnumerable<IPackageSearchMetadata> packages, bool outputWarning = false)
		{
			// Write warning message for Get-Package -ListAvaialble -Filter being obsolete
			// and will be replaced by Find-Package [-Id]
			VersionType versionType;
			string message;
			if (collapseVersions) {
				versionType = VersionType.Latest;
				message = "Find-Package [-Id]";
			} else {
				versionType = VersionType.All;
				message = "Find-Package [-Id] -AllVersions";
			}

			// Output list of PowerShellPackages
			if (outputWarning && !string.IsNullOrEmpty (Filter)) {
				Log (MessageLevel.Warning, GettextCatalog.GetString ("Command is obsolete", message));
			}

			WritePackages (packages, versionType);
		}

		void WritePackages (IEnumerable<IPackageSearchMetadata> packages, VersionType versionType)
		{
			List<PowerShellRemotePackage> view = PowerShellRemotePackage.GetPowerShellPackageView (packages, versionType);

			if (view.Any ()) {
				WritePackagesToOutputPipeline (view);
			} else {
				Log (MessageLevel.Info, GettextCatalog.GetString ("No packages available."));
			}
		}

		async Task WriteUpdatePackagesFromRemoteSourceAsyncInSolutionAsync ()
		{
			foreach (var project in projects) {
				await WriteUpdatePackagesFromRemoteSourceAsync (project);
			}
		}

		async Task WriteUpdatePackagesFromRemoteSourceAsync (NuGetProject project)
		{
			var frameworks = PowerShellCmdletsUtility.GetProjectTargetFrameworks (project);
			var installedPackages = await project.GetInstalledPackagesAsync (ConsoleHost.Token);

			VersionType versionType;
			if (collapseVersions) {
				versionType = VersionType.Latest;
			} else {
				versionType = VersionType.Updates;
			}

			bool projectHasUpdates = false;

			var metadataTasks = installedPackages.Select (
				installedPackage =>
				Task.Run (async () => {
					var metadata = await GetLatestPackageFromRemoteSourceAsync (project, installedPackage.PackageIdentity, IncludePrerelease.IsPresent);
					if (metadata != null) {
						await metadata.GetVersionsAsync ();
					}
					return metadata;
			}));

			foreach (var task in installedPackages.Zip (metadataTasks, (p, t) => Tuple.Create (t, p))) {
				var metadata = await task.Item1;

				if (metadata != null) {
					var package = PowerShellUpdatePackage.GetPowerShellPackageUpdateView (metadata, task.Item2.PackageIdentity.Version, versionType, project);

					var versions = package.Versions ?? Enumerable.Empty<NuGet.Versioning.NuGetVersion> ();
					if (versions.Any()) {
						projectHasUpdates = true;
						WriteObject (package);
					}
				}
			}

			if (!projectHasUpdates) {
				Log (MessageLevel.Info,
					GettextCatalog.GetString ("No package updates are available from the current package source for project '{0}'.", project.GetMetadata<string> (NuGetProjectMetadataKeys.Name)));
			}
		}
	}
}

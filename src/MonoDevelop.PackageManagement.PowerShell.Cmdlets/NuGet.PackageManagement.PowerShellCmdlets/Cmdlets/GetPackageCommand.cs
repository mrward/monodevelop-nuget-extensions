// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using MonoDevelop.PackageManagement;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.VisualStudio;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	/// <summary>
	/// This command lists the available packages which are either from a package source or installed in the
	/// current solution.
	/// </summary>
	[Cmdlet (VerbsCommon.Get, "Package", DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
	[OutputType (typeof (PowerShellPackage))]
	public class GetPackageCommand : NuGetPowerShellBaseCommand
	{
		const int DefaultFirstValue = 50;
		bool enablePaging;

		[Parameter (Position = 0)]
		[ValidateNotNullOrEmpty]
		public string Filter { get; set; }

		[Parameter (Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Project")]
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
		public virtual int First { get; set; }

		[Parameter]
		[ValidateRange (0, Int32.MaxValue)]
		public int Skip { get; set; }

		/// <summary>
		/// Determines if local repository are not needed to process this command
		/// </summary>
		protected bool UseRemoteSourceOnly { get; set; }

		/// <summary>
		/// Determines if a remote repository will be used to process this command.
		/// </summary>
		protected bool UseRemoteSource { get; set; }

		protected virtual bool CollapseVersions { get; set; }

		public List<NuGetProject> Projects { get; private set; }

		protected override bool IsLoggingTimeDisabled => true;

		void Preprocess ()
		{
			UseRemoteSourceOnly = ListAvailable.IsPresent || (!String.IsNullOrEmpty (Source) && !Updates.IsPresent);
			UseRemoteSource = ListAvailable.IsPresent || Updates.IsPresent || !String.IsNullOrEmpty (Source);
			CollapseVersions = !AllVersions.IsPresent;
			UpdateActiveSourceRepository (Source);

			NuGetUIThreadHelper.JoinableTaskFactory.Run (async () => {
				await GetNuGetProjectAsync (ProjectName);

				// When ProjectName is not specified, get all of the projects in the solution
				if (string.IsNullOrEmpty (ProjectName)) {
					var projects = await SolutionManager.GetAllNuGetProjectsAsync ();
					Projects = projects.ToList ();
				} else {
					Projects = new List<NuGetProject> { Project };
				}
			});
		}

		protected override void ProcessRecordCore ()
		{
			Preprocess ();

			// If Remote & Updates set of parameters are not specified, list the installed package.
			if (!UseRemoteSource) {
				CheckSolutionState ();

				var packagesToDisplay = NuGetUIThreadHelper.JoinableTaskFactory.Run (
					() => GetInstalledPackagesAsync (Projects, Filter, Skip, First, Token));

				WriteInstalledPackages (packagesToDisplay);
			} else {
				if (PageSize != 0) {
					enablePaging = true;
					First = PageSize;
				} else if (First == 0) {
					First = DefaultFirstValue;
				}

				if (Filter == null) {
					Filter = string.Empty;
				}

				// Find available packages from the current source and not taking targetframeworks into account.
				if (UseRemoteSourceOnly) {
					var errors = new List<string> ();
					var remotePackages = GetPackagesFromRemoteSource (Filter, IncludePrerelease.IsPresent, errors.Add)
						.Skip (Skip);

					// If there are any errors and there is only one source, then don't mention the
					// fact the no packages were found.
					var outputOnEmpty = PrimarySourceRepositories.Count () != 1 && errors.Any ();

					WritePackagesFromRemoteSource (
						remotePackages.Take (First),
						outputWarning: true,
						outputOnEmpty: outputOnEmpty);

					if (enablePaging) {
						WriteMoreRemotePackagesWithPaging (remotePackages.Skip (First));
					}

					foreach (var error in errors) {
						LogCore (MessageLevel.Error, error);
					}
				}
				// Get package udpates from the current source and taking targetframeworks into account.
				else {
					CheckSolutionState ();
					NuGetUIThreadHelper.JoinableTaskFactory.Run (WriteUpdatePackagesFromRemoteSourceAsyncInSolutionAsync);
				}
			}
		}

		async Task WriteUpdatePackagesFromRemoteSourceAsyncInSolutionAsync ()
		{
			foreach (var project in Projects) {
				await WriteUpdatePackagesFromRemoteSourceAsync (project);
			}
		}

		/// <summary>
		/// Output package updates to current project(s) found from the current remote source
		/// </summary>
		async Task WriteUpdatePackagesFromRemoteSourceAsync (NuGetProject project)
		{
			var installedPackages = await project.GetInstalledPackagesAsync (Token);
			installedPackages = installedPackages.Where (p => !IsAutoReferenced (p));

			VersionType versionType;
			if (CollapseVersions) {
				versionType = VersionType.Latest;
			} else {
				versionType = VersionType.Updates;
			}

			var projectHasUpdates = false;

			var metadataTasks = installedPackages.Select (installedPackage =>
				 Task.Run (async () => {
					 var metadata = await GetLatestPackageFromRemoteSourceAsync (installedPackage.PackageIdentity, IncludePrerelease.IsPresent);
					 if (metadata != null) {
						 await metadata.GetVersionsAsync ();
					 }
					 return metadata;
				 }));

			foreach (var task in installedPackages.Zip (metadataTasks, (p, t) => Tuple.Create (t, p))) {
				var metadata = await task.Item1;

				if (metadata != null) {
					var package = PowerShellUpdatePackage.GetPowerShellPackageUpdateView (metadata, task.Item2.PackageIdentity.Version, versionType, project);

					var versions = package.Versions ?? Enumerable.Empty<NuGetVersion> ();
					if (versions.Any ()) {
						projectHasUpdates = true;
						WriteObject (package);
					}
				}
			}

			if (!projectHasUpdates) {
				LogCore (MessageLevel.Info, string.Format (
					CultureInfo.CurrentCulture,
					"No package updates are available from the current package source for project '{0}'.",
					project.GetName ()));
			}
		}

		/// <summary>
		/// Output installed packages to the project(s)
		/// </summary>
		void WriteInstalledPackages (Dictionary<NuGetProject, IEnumerable<Packaging.PackageReference>> dictionary)
		{
			// Get the PowerShellPackageWithProjectView
			var view = PowerShellInstalledPackage.GetPowerShellPackageView (dictionary, null);
			if (view.Any ()) {
				WriteObject (view, enumerateCollection: true);
			} else {
				LogCore (MessageLevel.Info, "No packages installed.");
			}
		}

		/// <summary>
		/// Output packages found from the current remote source
		/// </summary>
		void WritePackagesFromRemoteSource (IEnumerable<IPackageSearchMetadata> packages, bool outputWarning, bool outputOnEmpty)
		{
			// Write warning message for Get-Package -ListAvaialble -Filter being obsolete
			// and will be replaced by Find-Package [-Id]
			VersionType versionType;
			string message;
			if (CollapseVersions) {
				versionType = VersionType.Latest;
				message = "Find-Package [-Id]";
			} else {
				versionType = VersionType.All;
				message = "Find-Package [-Id] -AllVersions";
			}

			// Output list of PowerShellPackages
			if (outputWarning && !string.IsNullOrEmpty (Filter)) {
				LogCore (MessageLevel.Warning, string.Format (
					CultureInfo.CurrentCulture,
					"This Command/Parameter combination has been deprecated and will be removed in the next release. Please consider using the new command that replaces it: '{0}'.",
					message));
			}

			WritePackages (packages, versionType, outputOnEmpty);
		}

		/// <summary>
		/// Output packages found from the current remote source with specified page size
		/// e.g. Get-Package -ListAvailable -PageSize 20
		/// </summary>
		void WriteMoreRemotePackagesWithPaging (IEnumerable<IPackageSearchMetadata> packagesToDisplay)
		{
			// Display more packages with paging
			foreach (var page in ToPagedCollection (packagesToDisplay, PageSize).Where (p => p.Any ())) {
				// Prompt to user and if want to continue displaying more packages
				int command = AskToContinueDisplayPackages ();
				if (command == 0) {
					// If yes, display the next page of (PageSize) packages
					WritePackagesFromRemoteSource (page, outputWarning: false, outputOnEmpty: false);
				} else {
					break;
				}
			}
		}

		static IEnumerable<IEnumerable<TSource>> ToPagedCollection<TSource> (IEnumerable<TSource> source, int pageSize)
		{
			var nextPage = new List<TSource> ();
			foreach (var item in source) {
				nextPage.Add (item);
				if (nextPage.Count == pageSize) {
					yield return nextPage;
					nextPage = new List<TSource> ();
				}
			}

			if (nextPage.Any ()) {
				yield return nextPage;
			}
		}

		void WritePackages (IEnumerable<IPackageSearchMetadata> packages, VersionType versionType, bool outputOnEmpty)
		{
			var view = PowerShellRemotePackage.GetPowerShellPackageView (packages, versionType);

			if (view.Any () || !outputOnEmpty) {
				WriteObject (view, enumerateCollection: true);
			} else {
				LogCore (MessageLevel.Info, "No packages found in the current package source.");
			}
		}

		int AskToContinueDisplayPackages ()
		{
			return 1;
			//// Add a line before message prompt
			//WriteLine ();
			//var choices = new Collection<ChoiceDescription>
			//	{
			//		new ChoiceDescription(Resources.Cmdlet_Yes, Resources.Cmdlet_DisplayMorePackagesYesHelp),
			//		new ChoiceDescription(Resources.Cmdlet_No, Resources.Cmdlet_DisplayMorePackagesNoHelp)
			//	};

			//var choice = Host.UI.PromptForChoice (string.Empty, Resources.Cmdlet_PrompToDisplayMorePackages, choices, defaultChoice: 1);

			//Debug.Assert (choice >= 0 && choice < 2);
			//// Add a line after
			//WriteLine ();
			//return choice;
		}

		static bool IsAutoReferenced (PackageReference reference)
		{
			return false;
			//return (reference as BuildIntegratedPackageReference)?.Dependency?.AutoReferenced == true;
		}
	}
}
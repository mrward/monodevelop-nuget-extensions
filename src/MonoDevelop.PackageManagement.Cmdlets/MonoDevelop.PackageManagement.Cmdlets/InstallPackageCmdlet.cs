// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement;
using NuGet.PackageManagement;
using NuGet.PackageManagement.PowerShellCmdlets;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGetVersion = NuGet.Versioning.NuGetVersion;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet (VerbsLifecycle.Install, "Package")]
	public class InstallPackageCmdlet : PackageActionBaseCmdlet
	{
		bool readFromPackagesConfig;
		bool readFromDirectPackagePath;
		NuGetVersion nugetVersion;
		bool versionSpecifiedPrerelease;
		bool allowPrerelease;
		bool isHttp;
		NuGetProject project;

		public InstallPackageCmdlet ()
			: this (
				PackageManagementExtendedServices.ConsoleHost,
				null)
		{
		}

		internal InstallPackageCmdlet (
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base (consoleHost, terminatingError)
		{
		}

		protected void Preprocess ()
		{
			ThrowErrorIfProjectNotOpen ();
			UpdateActiveSourceRepository (Source);
			project = ConsoleHost.GetNuGetProject (ProjectName);
			DetermineFileConflictAction ();

			ParseUserInputForId ();
			ParseUserInputForVersion ();

			// The following update to ActiveSourceRepository may get overwritten if the 'Id' was just a path to a nupkg
			if (readFromDirectPackagePath) {
				UpdateActiveSourceRepository (Source);
			}

			ActionType = NuGetActionType.Install;
		}

		protected override void ProcessRecord ()
		{
			Preprocess ();

			using (IConsoleHostFileConflictResolver resolver = CreateFileConflictResolver ()) {
				using (IDisposable monitor = CreateEventsMonitor ()) {
					InstallPackage ();
				}
			}
		}

		IConsoleHostFileConflictResolver CreateFileConflictResolver ()
		{
			return ConsoleHost.CreateFileConflictResolver (FileConflictAction);
		}

		void InstallPackage ()
		{
			if (!readFromPackagesConfig && !readFromDirectPackagePath && nugetVersion == null) {
				InstallPackageById ().Wait ();
			} else {
				IEnumerable<PackageIdentity> identities = GetPackageIdentities ();
				InstallPackages (identities).Wait ();
			}
		}

		/// <summary>
		/// Parse user input for Id parameter.
		/// Id can be the name of a package, path to packages.config file or path to .nupkg file.
		/// </summary>
		void ParseUserInputForId ()
		{
			if (!string.IsNullOrEmpty (Id)) {
				if (Id.EndsWith (Constants.PackageReferenceFile, StringComparison.OrdinalIgnoreCase)) {
					readFromPackagesConfig = true;
				} else if (Id.EndsWith (PackagingCoreConstants.NupkgExtension, StringComparison.OrdinalIgnoreCase)) {
					readFromDirectPackagePath = true;
					if (UriHelper.IsHttpSource (Id)) {
						isHttp = true;
						Source = Path.GetTempPath ();
					} else {
						string fullPath = Path.GetFullPath (Id);
						Source = Path.GetDirectoryName (fullPath);
					}
				}
			}
		}

		/// <summary>
		/// Parse user input for -Version switch.
		/// If Version is given as prerelease versions, automatically append -Prerelease
		/// </summary>
		void ParseUserInputForVersion ()
		{
			if (!string.IsNullOrEmpty (Version)) {
				nugetVersion = PowerShellCmdletsUtility.GetNuGetVersionFromString (Version);
				if (nugetVersion.IsPrerelease) {
					versionSpecifiedPrerelease = true;
				}
			}
			allowPrerelease = IncludePrerelease.IsPresent || versionSpecifiedPrerelease;
		}

		async Task InstallPackages (IEnumerable<PackageIdentity> identities)
		{
			foreach (PackageIdentity identity in identities) {
				await InstallPackageByIdentityAsync (project, identity, CreateResolutionContext (), this, WhatIf.IsPresent);
			}
		}

		Task InstallPackageById ()
		{
			return InstallPackageByIdAsync (project, Id, CreateResolutionContext (), this, WhatIf.IsPresent);
		}

		ResolutionContext CreateResolutionContext ()
		{
			return new ResolutionContext (GetDependencyBehavior(), allowPrerelease, false, VersionConstraints.None);
		}

		/// <summary>
		/// Returns list of package identities for Package Manager
		/// </summary>
		IEnumerable<PackageIdentity> GetPackageIdentities ()
		{
			IEnumerable<PackageIdentity> identityList = Enumerable.Empty<PackageIdentity> ();

			if (readFromPackagesConfig) {
				identityList = CreatePackageIdentitiesFromPackagesConfig ();
			} else if (readFromDirectPackagePath) {
				identityList = CreatePackageIdentityFromNupkgPath ();
			} else {
				identityList = GetPackageIdentity ();
			}

			return identityList;
		}

		/// <summary>
		/// Get the package identity to be installed based on package Id and Version
		/// </summary>
		IEnumerable<PackageIdentity> GetPackageIdentity ()
		{
			PackageIdentity identity = null;
			if (nugetVersion != null) {
				identity = new PackageIdentity (Id, nugetVersion);
			}
			return new List<PackageIdentity> { identity };
		}

		/// <summary>
		/// Return list of package identities parsed from packages.config
		/// </summary>
		IEnumerable<PackageIdentity> CreatePackageIdentitiesFromPackagesConfig ()
		{
			IEnumerable<PackageIdentity> identities = Enumerable.Empty<PackageIdentity> ();

			try {
				// Example: install-package https://raw.githubusercontent.com/NuGet/json-ld.net/master/src/JsonLD/packages.config
				if (UriHelper.IsHttpSource (Id)) {
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create (Id);
					HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
					// Read data via the response stream
					Stream resStream = response.GetResponseStream ();

					var reader = new PackagesConfigReader (resStream);
					var packageRefs = reader.GetPackages ();
					identities = packageRefs.Select (v => v.PackageIdentity);
				} else {
					// Example: install-package c:\temp\packages.config
					using (FileStream stream = new FileStream (Id, FileMode.Open)) {
						var reader = new PackagesConfigReader (stream);
						var packageRefs = reader.GetPackages ();
						identities = packageRefs.Select (v => v.PackageIdentity);
					}
				}

				// Set _allowPrerelease to true if any of the identities is prerelease version.
				if (identities != null
					&& identities.Any ()) {
					foreach (PackageIdentity identity in identities) {
						if (identity.Version.IsPrerelease) {
							allowPrerelease = true;
							break;
						}
					}
				}
			} catch (Exception ex) {
				Log (MessageLevel.Error, GettextCatalog.GetString ("Failed to parse package identities from file {0} with exception: {1}", Id, ex.Message));
			}

			return identities;
		}

		/// <summary>
		/// Return package identity parsed from path to .nupkg file
		/// </summary>
		IEnumerable<PackageIdentity> CreatePackageIdentityFromNupkgPath ()
		{
			PackageIdentity identity = null;

			try {
				// Example: install-package https://az320820.vo.msecnd.net/packages/microsoft.aspnet.mvc.4.0.20505.nupkg
				if (isHttp) {
					identity = ParsePackageIdentityFromNupkgPath (Id, @"/");
					if (identity != null) {
						Directory.CreateDirectory (Source);
						string downloadPath = Path.Combine (Source, identity + PackagingCoreConstants.NupkgExtension);

						using (var client = new System.Net.Http.HttpClient ()) {
							Task.Run (async delegate {
								using (Stream downloadStream = await client.GetStreamAsync (Id)) {
									using (var targetPackageStream = new FileStream (downloadPath, FileMode.Create, FileAccess.Write)) {
										await downloadStream.CopyToAsync (targetPackageStream);
									}
								}
							}).Wait ();
						}
					}
				} else {
					// Example: install-package c:\temp\packages\jQuery.1.10.2.nupkg
					identity = ParsePackageIdentityFromNupkgPath (Id, @"\");
				}

				// Set _allowPrerelease to true if identity parsed is prerelease version.
				if (identity != null
					&& identity.Version != null
					&& identity.Version.IsPrerelease) {
					allowPrerelease = true;
				}
			} catch (Exception ex) {
				Log (MessageLevel.Error, GettextCatalog.GetString ("Failed to parse package identities from file {0} with exception: {1}", Id, ex.Message));
			}

			return new List<PackageIdentity> { identity };
		}

		/// <summary>
		/// Parse package identity from path to .nupkg file, such as
		/// https://az320820.vo.msecnd.net/packages/microsoft.aspnet.mvc.4.0.20505.nupkg
		/// </summary>
		PackageIdentity ParsePackageIdentityFromNupkgPath (string path, string divider)
		{
			if (!string.IsNullOrEmpty (path)) {
				string lastPart = path.Split (new [] { divider }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault ();
				lastPart = lastPart.Replace (PackagingCoreConstants.NupkgExtension, "");
				string [] parts = lastPart.Split (new [] { "." }, StringSplitOptions.RemoveEmptyEntries);
				var builderForId = new StringBuilder ();
				var builderForVersion = new StringBuilder ();
				foreach (string s in parts) {
					int n;
					bool isNumeric = int.TryParse (s, out n);
					// Take pre-release versions such as EntityFramework.6.1.3-beta1 into account.
					if ((!isNumeric || string.IsNullOrEmpty (builderForId.ToString ()))
						&& string.IsNullOrEmpty (builderForVersion.ToString ())) {
						builderForId.Append (s);
						builderForId.Append (".");
					} else {
						builderForVersion.Append (s);
						builderForVersion.Append (".");
					}
				}
				NuGetVersion nVersion = PowerShellCmdletsUtility.GetNuGetVersionFromString (builderForVersion.ToString ().TrimEnd ('.'));
				// Set _allowPrerelease to true if nVersion is prerelease version.
				if (nVersion != null && nVersion.IsPrerelease) {
					allowPrerelease = true;
				}
				return new PackageIdentity (builderForId.ToString ().TrimEnd ('.'), nVersion);
			}
			return null;
		}
	}
}

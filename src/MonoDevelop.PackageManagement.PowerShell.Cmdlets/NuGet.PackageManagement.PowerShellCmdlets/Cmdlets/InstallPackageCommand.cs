// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.Threading;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Versioning;
using Task = System.Threading.Tasks.Task;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	[Cmdlet (VerbsLifecycle.Install, "Package")]
	public class InstallPackageCommand : PackageActionBaseCommand
	{
		bool readFromPackagesConfig;
		bool readFromDirectPackagePath;
		NuGetVersion nugetVersion;
		bool versionSpecifiedPrerelease;
		bool allowPrerelease;
		bool isHttp;

		protected override void Preprocess ()
		{
			base.Preprocess ();
			ParseUserInputForId ();
			ParseUserInputForVersion ();

			// The following update to ActiveSourceRepository may get overwritten if the 'Id' was just a path to a nupkg
			if (readFromDirectPackagePath) {
				UpdateActiveSourceRepository (Source);
			}
		}

		protected override void ProcessRecordCore ()
		{
			Preprocess ();

			WarnIfParametersAreNotSupported ();

			if (!readFromPackagesConfig
				&& !readFromDirectPackagePath
				&& nugetVersion == null) {
				Task.Run (InstallPackageByIdAsync).Forget ();
			} else {
				var identities = GetPackageIdentities ();
				Task.Run (() => InstallPackagesAsync (identities)).Forget ();
			}
			WaitAndLogPackageActions ();
		}

		/// <summary>
		/// Async call for install packages from the list of identities.
		/// </summary>
		async Task InstallPackagesAsync (IEnumerable<PackageIdentity> identities)
		{
			try {
				foreach (var identity in identities) {
					await InstallPackageByIdentityAsync (DTEProject, identity,GetDependencyBehavior (), allowPrerelease, WhatIf.IsPresent);
				}
			} catch (Exception ex) {
				Log (MessageLevel.Error, ExceptionUtilities.DisplayMessage (ex));
			} finally {
				BlockingCollection.Add (new ExecutionCompleteMessage ());
			}
		}

		/// <summary>
		/// Async call for install a package by Id.
		/// </summary>
		async Task InstallPackageByIdAsync ()
		{
			try {
				await InstallPackageByIdAsync (DTEProject, Id, GetDependencyBehavior (), allowPrerelease, WhatIf.IsPresent);
			} catch (Exception ex) {
				Log (MessageLevel.Error, ExceptionUtilities.DisplayMessage (ex));
			} finally {
				BlockingCollection.Add (new ExecutionCompleteMessage ());
			}
		}

		/// <summary>
		/// Parse user input for Id parameter.
		/// Id can be the name of a package, path to packages.config file or path to .nupkg file.
		/// </summary>
		void ParseUserInputForId ()
		{
			if (!string.IsNullOrEmpty (Id)) {
				if (Id.EndsWith (NuGetConstants.PackageReferenceFile, StringComparison.OrdinalIgnoreCase)) {
					readFromPackagesConfig = true;
				} else if (Id.EndsWith (PackagingCoreConstants.NupkgExtension, StringComparison.OrdinalIgnoreCase)) {
					readFromDirectPackagePath = true;
					if (UriHelper.IsHttpSource (Id)) {
						isHttp = true;
						Source = Path.GetTempPath ();
					} else {
						var fullPath = Path.GetFullPath (Id);
						Source = Path.GetDirectoryName (fullPath);
					}
				} else {
					NormalizePackageId (Project);
				}
			}
		}

		/// <summary>
		/// Returns list of package identities for Package Manager
		/// </summary>
		/// <returns></returns>
		IEnumerable<PackageIdentity> GetPackageIdentities ()
		{
			var identityList = Enumerable.Empty<PackageIdentity> ();

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
		/// <returns></returns>
		[SuppressMessage ("Microsoft.Design", "CA1031")]
		IEnumerable<PackageIdentity> CreatePackageIdentitiesFromPackagesConfig ()
		{
			var identities = Enumerable.Empty<PackageIdentity> ();

			try {
				// Example: install-package https://raw.githubusercontent.com/NuGet/json-ld.net/master/src/JsonLD/packages.config
				if (UriHelper.IsHttpSource (Id)) {
					var request = (HttpWebRequest)WebRequest.Create (Id);
					var response = (HttpWebResponse)request.GetResponse ();
					// Read data via the response stream
					var resStream = response.GetResponseStream ();

					var reader = new PackagesConfigReader (resStream);
					var packageRefs = reader.GetPackages ();
					identities = packageRefs.Select (v => v.PackageIdentity);
				} else {
					// Example: install-package c:\temp\packages.config
					using (var stream = new FileStream (Id, FileMode.Open)) {
						var reader = new PackagesConfigReader (stream);
						var packageRefs = reader.GetPackages ();
						identities = packageRefs.Select (v => v.PackageIdentity);
					}
				}

				// Set _allowPrerelease to true if any of the identities is prerelease version.
				if (identities != null
					&& identities.Any ()) {
					foreach (var identity in identities) {
						if (identity.Version.IsPrerelease) {
							allowPrerelease = true;
							break;
						}
					}
				}
			} catch (Exception ex) {
				LogCore (MessageLevel.Error, string.Format (
					CultureInfo.CurrentCulture,
					"Failed to parse package identities from file {0} with exception: {1}",
					Id,
					ex.Message));
			}

			return identities;
		}

		/// <summary>
		/// Return package identity parsed from path to .nupkg file
		/// </summary>
		/// <returns></returns>
		[SuppressMessage ("Microsoft.Design", "CA1031")]
		private IEnumerable<PackageIdentity> CreatePackageIdentityFromNupkgPath ()
		{
			PackageIdentity identity = null;

			try {
				// Example: install-package https://az320820.vo.msecnd.net/packages/microsoft.aspnet.mvc.4.0.20505.nupkg
				if (isHttp) {
					identity = ParsePackageIdentityFromNupkgPath (Id, @"/");
					if (identity != null) {
						Directory.CreateDirectory (Source);
						var downloadPath = Path.Combine (Source, identity + PackagingCoreConstants.NupkgExtension);

						using (var client = new System.Net.Http.HttpClient ()) {
							Task.Run (async delegate {
								using (var downloadStream = await client.GetStreamAsync (Id)) {
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
				Log (MessageLevel.Error, "Failed to parse package identities from file {0} with exception: {1}", Id, ex.Message);
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
				var lastPart = path.Split (new[] { divider }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault ();
				lastPart = lastPart.Replace (PackagingCoreConstants.NupkgExtension, "");
				var parts = lastPart.Split (new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
				var builderForId = new StringBuilder ();
				var builderForVersion = new StringBuilder ();
				foreach (var s in parts) {
					int n;
					var isNumeric = int.TryParse (s, out n);
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
				var nVersion = PowerShellCmdletsUtility.GetNuGetVersionFromString (builderForVersion.ToString ().TrimEnd ('.'));
				// Set _allowPrerelease to true if nVersion is prerelease version.
				if (nVersion != null
					&& nVersion.IsPrerelease) {
					allowPrerelease = true;
				}
				return new PackageIdentity (builderForId.ToString ().TrimEnd ('.'), nVersion);
			}
			return null;
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
	}
}
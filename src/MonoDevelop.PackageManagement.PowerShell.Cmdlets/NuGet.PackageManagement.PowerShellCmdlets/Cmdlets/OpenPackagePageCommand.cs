// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	[Cmdlet (VerbsCommon.Open, "PackagePage", DefaultParameterSetName = ParameterAttribute.AllParameterSets, SupportsShouldProcess = true)]
	public class OpenPackagePageCommand : NuGetPowerShellBaseCommand
	{
		[Parameter (Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
		public string Id { get; set; }

		[Parameter (Position = 1)]
		[ValidateNotNull]
		public string Version { get; set; }

		[Parameter]
		[ValidateNotNullOrEmpty]
		public virtual string Source { get; set; }

		[Parameter (Mandatory = true, ParameterSetName = "License")]
		public SwitchParameter License { get; set; }

		[Parameter (Mandatory = true, ParameterSetName = "ReportAbuse")]
		public SwitchParameter ReportAbuse { get; set; }

		[Parameter]
		public SwitchParameter PassThru { get; set; }

		[Parameter]
		public SwitchParameter IncludePrerelease { get; set; }

		private void Preprocess ()
		{
			UpdateActiveSourceRepository (Source);
			string message = "The {0} Command has been deprecated and will be removed in the next release. Please modify your PowerShell scripts accordingly. ";
			LogCore (MessageLevel.Warning, string.Format (CultureInfo.CurrentCulture, message, "Open-PackagePage"));
		}

		[SuppressMessage ("Microsoft.Design", "CA1031")]
		protected override void ProcessRecordCore ()
		{
			Preprocess ();

			IPackageSearchMetadata package = null;
			try {
				var metadata = Task.Run (() => GetPackagesFromRemoteSourceAsync (Id, IncludePrerelease.IsPresent)).Result;

				if (!string.IsNullOrEmpty (Version)) {
					NuGetVersion nVersion = PowerShellCmdletsUtility.GetNuGetVersionFromString (Version);
					metadata = metadata.Where (p => p.Identity.Version == nVersion);
				}
				package = metadata
					.OrderByDescending (v => v.Identity.Version)
					.FirstOrDefault ();
			} catch {
			}

			if (package != null
				&& package.Identity != null) {
				Uri targetUrl = null;
				if (License.IsPresent) {
					targetUrl = package.LicenseUrl;
				} else if (ReportAbuse.IsPresent) {
					targetUrl = package.ReportAbuseUrl;
				} else {
					targetUrl = package.ProjectUrl;
				}

				if (targetUrl != null) {
					OpenUrl (targetUrl);

					if (PassThru.IsPresent) {
						WriteObject (targetUrl);
					}
				} else {
					WriteError (String.Format (
						CultureInfo.CurrentCulture,
						"The package '{0}' does not provide the requested URL.",
						package.Identity.Id + " " + package.Identity.Version));
				}
			} else {
				// show appropriate error message depending on whether Version parameter is set.
				if (string.IsNullOrEmpty (Version)) {
					WriteError (String.Format (
						CultureInfo.CurrentCulture,
						"Package with the Id '{0}' is not found in the specified source.",
						Id));
				} else {
					WriteError (String.Format (
						CultureInfo.CurrentCulture,
						"Package with the Id '{0}' and version '{1}' is not found in the specified source.",
						Id,
						Version));
				}
			}
		}

		void OpenUrl (Uri targetUrl)
		{
			// ask for confirmation or if WhatIf is specified
			if (ShouldProcess (targetUrl.OriginalString, "Open")) {
				LogCore(MessageLevel.Info, string.Format ("Url: '{0}'", targetUrl.AbsoluteUri));
				UriLauncher.OpenExternalLink (targetUrl);
			}
		}
	}
}
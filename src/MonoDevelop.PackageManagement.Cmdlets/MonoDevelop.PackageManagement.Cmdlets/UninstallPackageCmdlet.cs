// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.PackageManagement;
using MonoDevelop.PackageManagement.Scripting;
using NuGet;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet (VerbsLifecycle.Uninstall, "Package")]
	public class UninstallPackageCmdlet : PackageManagementCmdlet
	{
		public UninstallPackageCmdlet ()
			: this (
				PackageManagementExtendedServices.ConsoleHost,
				null)
		{
		}

		internal UninstallPackageCmdlet (
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base (consoleHost, terminatingError)
		{
		}

		[Parameter (Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
		public string Id { get; set; }

		[Parameter (Position = 1, ValueFromPipelineByPropertyName = true)]
		[ValidateNotNullOrEmpty]
		public string ProjectName { get; set; }

		[Parameter (Position = 2)]
		[ValidateNotNullOrEmpty]
		public SemanticVersion Version { get; set; }

		[Parameter]
		public SwitchParameter WhatIf { get; set; }

		[Parameter]
		public SwitchParameter Force { get; set; }

		[Parameter]
		public SwitchParameter RemoveDependencies { get; set; }

		protected override void ProcessRecord ()
		{
			ActionType = NuGetActionType.Uninstall;

			ThrowErrorIfProjectNotOpen ();

			using (IDisposable monitor = CreateEventsMonitor ()) {
				UninstallPackage ();
			}
		}

		void UninstallPackage ()
		{
			NuGetProject project = ConsoleHost.GetNuGetProject (ProjectName);
			UninstallPackageByIdAsync (project, Id, CreateUninstallContext (), this, WhatIf.IsPresent).Wait ();
		}

		protected async Task UninstallPackageByIdAsync (
			NuGetProject project,
			string packageId,
			UninstallationContext uninstallContext,
			INuGetProjectContext projectContext,
			bool isPreview)
		{
			ConsoleHostNuGetPackageManager packageManager = ConsoleHost.CreatePackageManager ();
			IEnumerable<NuGetProjectAction> actions = await packageManager.PreviewUninstallPackageAsync (project, packageId, uninstallContext, projectContext, ConsoleHost.Token);

			if (isPreview) {
				PreviewNuGetPackageActions (actions);
			} else {
				await packageManager.ExecuteNuGetProjectActionsAsync (project, actions, projectContext, ConsoleHost.Token);
			}
		}

		UninstallationContext CreateUninstallContext ()
		{
			return new UninstallationContext (RemoveDependencies.IsPresent, Force.IsPresent);
		}
	}
}

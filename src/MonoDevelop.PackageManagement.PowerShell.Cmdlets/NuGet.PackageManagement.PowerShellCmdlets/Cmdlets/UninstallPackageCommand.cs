// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Management.Automation;
using System.Threading;
using Microsoft.VisualStudio.Threading;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;
using NuGet.Common;
using NuGet.ProjectManagement;
using Task = System.Threading.Tasks.Task;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	[Cmdlet (VerbsLifecycle.Uninstall, "Package")]
	public class UninstallPackageCommand : NuGetPowerShellBaseCommand
	{
		UninstallationContext context;

		[Parameter (Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
		public virtual string Id { get; set; }

		[Parameter (ValueFromPipelineByPropertyName = true, Position = 1)]
		[ValidateNotNullOrEmpty]
		public virtual string ProjectName { get; set; }

		[Parameter (Position = 2)]
		[ValidateNotNullOrEmpty]
		public virtual string Version { get; set; }

		[Parameter]
		public SwitchParameter WhatIf { get; set; }

		[Parameter]
		public SwitchParameter Force { get; set; }

		[Parameter]
		public SwitchParameter RemoveDependencies { get; set; }

		void Preprocess ()
		{
			CheckSolutionState ();

			Task.Run (async () => {
				await GetDTEProjectAsync (ProjectName);
				//await CheckMissingPackagesAsync ();
			}).Wait ();
		}

		protected override void ProcessRecordCore ()
		{
			Preprocess ();

			UninstallPackageAsync ().Forget ();
			WaitAndLogPackageActions ();
		}

		/// <summary>
		/// Async call for uninstall a package from the current project
		/// </summary>
		async Task UninstallPackageAsync ()
		{
			try {
				await UninstallPackageByIdAsync (DTEProject, Id, UninstallContext, WhatIf.IsPresent);
			} catch (Exception ex) {
				Log (MessageLevel.Error, ExceptionUtilities.DisplayMessage (ex));
			} finally {
				BlockingCollection.Add (new ExecutionCompleteMessage ());
			}
		}

		/// <summary>
		/// Uninstall package by Id
		/// </summary>
		/// <param name="project"></param>
		/// <param name="packageId"></param>
		/// <param name="uninstallContext"></param>
		/// <param name="isPreview"></param>
		protected async Task UninstallPackageByIdAsync (Project project, string packageId, UninstallationContext uninstallContext, bool isPreview)
		{
			if (isPreview) {
				var actions = await project.GetUninstallPackageActionsAsync (packageId, uninstallContext, Token);
				PreviewNuGetPackageActions (actions);
			} else {
				await project.UninstallPackageAsync (packageId, uninstallContext, CancellationToken.None);
			}
		}

		/// <summary>
		/// Uninstallation Context for Uninstall-Package command
		/// </summary>
		public UninstallationContext UninstallContext {
			get {
				context = new UninstallationContext (RemoveDependencies.IsPresent, Force.IsPresent);
				return context;
			}
		}
	}
}
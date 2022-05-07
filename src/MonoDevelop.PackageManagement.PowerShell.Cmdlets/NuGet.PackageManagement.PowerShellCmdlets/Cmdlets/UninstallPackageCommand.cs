// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Management.Automation;
using System.Threading;
using Microsoft.VisualStudio.Threading;
using NuGet.Common;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.VisualStudio;
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

			NuGetUIThreadHelper.JoinableTaskFactory.Run (async () => {
				await GetNuGetProjectAsync (ProjectName);
				//await CheckMissingPackagesAsync ();
			});

			ActionType = NuGetActionType.Uninstall;
		}

		protected override void ProcessRecordCore ()
		{
			Preprocess ();

			NuGetUIThreadHelper.JoinableTaskFactory.Run (() => {
				Task.Run (UninstallPackageAsync).Forget ();
				WaitAndLogPackageActions ();

				return Task.FromResult (true);
			});
		}

		/// <summary>
		/// Async call for uninstall a package from the current project
		/// </summary>
		async Task UninstallPackageAsync ()
		{
			try {
				await UninstallPackageByIdAsync (Project, Id, UninstallContext, this, WhatIf.IsPresent);
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
		protected async Task UninstallPackageByIdAsync (NuGetProject project, string packageId, UninstallationContext uninstallContext, INuGetProjectContext projectContext, bool isPreview)
		{
			var actions = await PackageManager.PreviewUninstallPackageAsync (project, packageId, uninstallContext, projectContext, CancellationToken.None);

			if (isPreview) {
				PreviewNuGetPackageActions (actions);
			} else {
				await PackageManager.ExecuteNuGetProjectActionsAsync (project, actions, projectContext, NullSourceCacheContext.Instance, CancellationToken.None);
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
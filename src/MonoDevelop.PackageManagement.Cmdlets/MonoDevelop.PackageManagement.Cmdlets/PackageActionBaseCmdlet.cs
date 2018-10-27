// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Management.Automation;
using System.Threading;
using ICSharpCode.PackageManagement.Cmdlets;
using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.Core;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Resolver;

using Task = System.Threading.Tasks.Task;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	public class PackageActionBaseCmdlet : PackageManagementCmdlet
	{
		internal PackageActionBaseCmdlet(
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base (consoleHost, terminatingError)
		{
		}

		[Parameter (Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
		public virtual string Id { get; set; }

		[Parameter (ValueFromPipelineByPropertyName = true, Position = 1)]
		[ValidateNotNullOrEmpty]
		public virtual string ProjectName { get; set; }

		[Parameter (Position = 2)]
		[ValidateNotNullOrEmpty]
		public virtual string Version { get; set; }

		[Parameter (Position = 3)]
		[ValidateNotNullOrEmpty]
		public virtual string Source { get; set; }

		[Parameter]
		public SwitchParameter WhatIf { get; set; }

		[Parameter]
		[Alias ("Prerelease")]
		public SwitchParameter IncludePrerelease { get; set; }

		[Parameter]
		public SwitchParameter IgnoreDependencies { get; set; }

		[Parameter]
		public FileConflictAction? FileConflictAction { get; set; }

		[Parameter]
		public DependencyBehavior? DependencyVersion { get; set; }

		protected async Task InstallPackageByIdentityAsync (
			NuGetProject project,
			PackageIdentity identity,
			ResolutionContext resolutionContext,
			INuGetProjectContext projectContext,
			bool isPreview)
		{
			try {
				var packageManager = ConsoleHost.CreatePackageManager ();

				var actions = await packageManager.PreviewInstallPackageAsync (
					project,
					identity,
					resolutionContext,
					projectContext,
					PrimarySourceRepositories,
					null,
					ConsoleHost.Token);

				if (isPreview) {
					PreviewNuGetPackageActions (actions);
				} else {
					NuGetPackageManager.SetDirectInstall (identity, projectContext);
					await packageManager.ExecuteNuGetProjectActionsAsync (
						project,
						actions,
						this,
						resolutionContext.SourceCacheContext,
						ConsoleHost.Token);
					NuGetPackageManager.ClearDirectInstall (projectContext);
				}
			} catch (InvalidOperationException ex) {
				if (ex.InnerException is PackageAlreadyInstalledException) {
					Log (ProjectManagement.MessageLevel.Info, ex.Message);
				} else {
					throw ex;
				}
			}
		}

		protected async Task InstallPackageByIdAsync (
			NuGetProject project,
			string packageId,
			ResolutionContext resolutionContext,
			INuGetProjectContext projectContext,
			bool isPreview)
		{
			try {
				var packageManager = ConsoleHost.CreatePackageManager ();

				var latestVersion = await packageManager.GetLatestVersionAsync (
					packageId,
					project,
					resolutionContext,
					PrimarySourceRepositories,
					new LoggerAdapter (projectContext),
					ConsoleHost.Token);

				if (latestVersion == null) {
					throw new InvalidOperationException (GettextCatalog.GetString ("Unable to find package '{0}", packageId));
				}

				var identity = new PackageIdentity (Id, latestVersion.LatestVersion);

				var actions = await packageManager.PreviewInstallPackageAsync (
					project,
					identity,
					resolutionContext,
					projectContext,
					PrimarySourceRepositories,
					null,
					CancellationToken.None);

				if (isPreview) {
					PreviewNuGetPackageActions (actions);
				} else {
					NuGetPackageManager.SetDirectInstall (identity, projectContext);
					await packageManager.ExecuteNuGetProjectActionsAsync (
						project,
						actions,
						this,
						resolutionContext.SourceCacheContext,
						ConsoleHost.Token);
					NuGetPackageManager.ClearDirectInstall (projectContext);
				}
			} catch (InvalidOperationException ex) {
				if (ex.InnerException is PackageAlreadyInstalledException) {
					Log (ProjectManagement.MessageLevel.Info, ex.Message);
				} else {
					throw ex;
				}
			}
		}

		protected void DetermineFileConflictAction ()
		{
			if (FileConflictAction != null) {
				ConflictAction = FileConflictAction;
			}
		}

		protected virtual DependencyBehavior GetDependencyBehavior ()
		{
			if (IgnoreDependencies.IsPresent) {
				return DependencyBehavior.Ignore;
			}
			if (DependencyVersion.HasValue) {
				return DependencyVersion.Value;
			}
			return GetDependencyBehaviorFromConfig ();
		}

		/// <summary>
		/// Get the value of DependencyBehavior from NuGet.Config file
		/// </summary>
		protected DependencyBehavior GetDependencyBehaviorFromConfig ()
		{
			string dependencySetting = ConsoleHost.Settings.GetValue ("config", "dependencyversion");
			DependencyBehavior behavior;
			bool success = Enum.TryParse (dependencySetting, true, out behavior);
			if (success) {
				return behavior;
			}
			// Default to Lowest
			return DependencyBehavior.Lowest;
		}
	}
}

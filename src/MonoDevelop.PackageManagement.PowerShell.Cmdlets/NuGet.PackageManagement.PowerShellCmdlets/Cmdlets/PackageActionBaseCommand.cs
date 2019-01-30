// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.Resolver;
using Task = System.Threading.Tasks.Task;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	public class PackageActionBaseCommand : NuGetPowerShellBaseCommand
	{
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

		protected virtual void Preprocess ()
		{
			CheckSolutionState ();

			var result = ValidateSource (Source);
			if (result.Validity == SourceValidity.UnknownSource) {
				throw new PackageSourceException (string.Format (
					CultureInfo.CurrentCulture,
					"Unable to find package '{0}' at source '{1}'. Source not found.",
					Id,
					result.Source));
			} else if (result.Validity == SourceValidity.UnknownSourceType) {
				throw new PackageSourceException (string.Format (
					CultureInfo.CurrentCulture,
					"Unsupported type of source '{0}'. Please provide an HTTP or local source.",
					result.Source));
			}

			UpdateActiveSourceRepository (result.SourceRepository);
			DetermineFileConflictAction ();

			Task.Run (async delegate {
				await GetDTEProjectAsync (ProjectName);
				//await CheckMissingPackagesAsync ();
				//await CheckPackageManagementFormat ();
			}).Wait ();
		}

		protected override void ProcessRecordCore ()
		{
		}

		/// <summary>
		/// Install package by Identity
		/// </summary>
		protected async Task InstallPackageByIdentityAsync (
			Project project,
			PackageIdentity identity,
			DependencyBehavior dependencyBehavior,
			bool allowPrerelease,
			bool isPreview)
		{
			if (isPreview) {
				var actions = await project.PreviewInstallPackageAsync (
					identity.Id,
					identity.Version.ToNormalizedString (),
					dependencyBehavior,
					allowPrerelease,
					PrimarySourceRepositories,
					Token);
				PreviewNuGetPackageActions (actions);
			} else {
				await project.InstallPackageAsync (
					identity.Id,
					identity.Version.ToNormalizedString (),
					dependencyBehavior,
					allowPrerelease,
					PrimarySourceRepositories,
					Token);
			}
		}

		/// <summary>
		/// Install package by Id
		/// </summary>
		protected async Task InstallPackageByIdAsync (
			Project project,
			string packageId,
			DependencyBehavior dependencyBehavior,
			bool allowPrerelease,
			bool isPreview)
		{
			//if (!ShouldContinueDueToDotnetDeprecation (actions, isPreview)) {
			//	return;
			//}

			if (isPreview) {
				var actions = await project.PreviewInstallPackageAsync (
					packageId,
					null,
					dependencyBehavior,
					allowPrerelease,
					PrimarySourceRepositories,
					Token);
				PreviewNuGetPackageActions (actions);
			} else {
				await project.InstallPackageAsync (
					packageId,
					null,
					dependencyBehavior,
					allowPrerelease,
					PrimarySourceRepositories,
					Token);
			}
		}

		/// <summary>
		/// Normalize package Id input against server metadata for project K, which is case-sensitive.
		/// </summary>
		protected void NormalizePackageId (NuGetProject project)
		{
			if (!(project is ProjectKNuGetProjectBase)) {
				return;
			}

			var metadata = Task.Run (() => GetPackagesFromRemoteSourceAsync (Id, includePrerelease: true)).Result;
			if (!metadata.Any ()) {
				return;
			}

			// note that we're assuming that package id is the same for all versions.
			Id = metadata.First ().Identity.Id;
		}

		protected virtual void WarnIfParametersAreNotSupported ()
		{
			if (Source != null && Project is BuildIntegratedNuGetProject) {
				var warning = string.Format (
					CultureInfo.CurrentUICulture,
					"The '{0}' parameter is not respected for the transitive package management based project(s) {1}. The enabled sources in your NuGet configuration will be used.",
					nameof (Source),
					NuGetProject.GetUniqueNameOrName (Project));
				Log (MessageLevel.Warning, warning);
			}
		}

		//protected async Task CheckPackageManagementFormat ()
		//{
			//// check if Project has any packages installed
			//if (!(await Project.GetInstalledPackagesAsync (Token)).Any ()) {
			//	var packageManagementFormat = new PackageManagementFormat (ConfigSettings);

			//	// if default format is PackageReference then update NuGet Project
			//	if (packageManagementFormat.SelectedPackageManagementFormat == 1) {
			//		var newProject = await VsSolutionManager.UpgradeProjectToPackageReferenceAsync (Project);

			//		if (newProject != null) {
			//			Project = newProject;
			//		}
			//	}
			//}
		//}

		protected bool ShouldContinueDueToDotnetDeprecation (IEnumerable<NuGetProjectAction> actions, bool whatIf)
		{
			//// Don't prompt if the user has chosen to ignore this warning.
			//// Also, -WhatIf should not prompt the user since there is not action performed anyway.
			//if (DotnetDeprecatedPrompt.GetDoNotShowPromptState () || whatIf) {
			//	return true;
			//}

			//// Determine if any of the project actions are affected by the deprecated framework.
			//var resolvedActions = actions.Select (action => new ResolvedAction (action.Project, action));

			//var projects = DotnetDeprecatedPrompt.GetAffectedProjects (resolvedActions);
			//if (!projects.Any ()) {
			//	return true;
			//}

			//var model = DotnetDeprecatedPrompt.GetDeprecatedFrameworkModel (projects);

			//// Flush the existing messages (e.g. logging from the action preview).
			//FlushBlockingCollection ();

			//// Prompt the user to determine if the project actions should be executed.
			//var choices = new Collection<ChoiceDescription>
			//{
			//	new ChoiceDescription(Resources.Cmdlet_No, Resources.Cmdlet_DeprecatedFrameworkNoHelp),
			//	new ChoiceDescription(Resources.Cmdlet_Yes, Resources.Cmdlet_DeprecatedFrameworkYesHelp),
			//	new ChoiceDescription(Resources.Cmdlet_YesDoNotPromptAgain, Resources.Cmdlet_DeprecatedFrameworkYesDoNotPromptAgainHelp)
			//};

			//var messageBuilder = new StringBuilder ();
			//messageBuilder.Append (model.TextBeforeLink);
			//messageBuilder.Append (model.LinkText);
			//messageBuilder.Append (" (");
			//messageBuilder.Append (model.MigrationUrl);
			//messageBuilder.Append (")");
			//messageBuilder.Append (model.TextAfterLink);
			//messageBuilder.Append (" ");
			//messageBuilder.Append (Resources.Cmdlet_DeprecatedFrameworkContinue);

			//WriteLine ();
			//var choice = Host.UI.PromptForChoice (
			//	string.Empty,
			//	messageBuilder.ToString (),
			//	choices,
			//	defaultChoice: 0);
			//WriteLine ();

			//// Handle the response from the user.
			//if (choice == 2) {
			//	// Yes and do not prompt again.
			//	DotnetDeprecatedPrompt.SaveDoNotShowPromptState (true);
			//	return true;
			//} else if (choice == 1) // Yes
			//  {
			//	return true;
			//}

			return false; // No
		}

		/// <summary>
		/// Determine file confliction action based on user input
		/// </summary>
		void DetermineFileConflictAction ()
		{
			//if (FileConflictAction != null) {
			//	ConflictAction = FileConflictAction;
			//}
		}

		/// <summary>
		/// Determine DependencyBehavior based on user input
		/// </summary>
		/// <returns></returns>
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
		/// <returns></returns>
		protected DependencyBehavior GetDependencyBehaviorFromConfig ()
		{
			//string dependencySetting = ConfigSettings.GetValue ("config", "dependencyversion");
			//DependencyBehavior behavior;
			//bool success = Enum.TryParse (dependencySetting, true, out behavior);
			//if (success) {
			//	return behavior;
			//}
			// Default to Lowest
			return DependencyBehavior.Lowest;
		}
	}
}
// 
// UpdatePackageCmdlet.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Management.Automation;
using ICSharpCode.PackageManagement.Scripting;
using NuGet;
using MonoDevelop.PackageManagement;
using NuGet.ProjectManagement;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet (VerbsData.Update, "Package", DefaultParameterSetName = "All")]
	public class UpdatePackageCmdlet : PackageManagementCmdlet
	{
//		IUpdatePackageActionsFactory updatePackageActionsFactory;

//		public UpdatePackageCmdlet ()
//			: this (
//				new UpdatePackageActionsFactory (),
//				PackageManagementExtendedServices.ConsoleHost,
//				null)
//		{
//		}

		internal UpdatePackageCmdlet (
//			IUpdatePackageActionsFactory updatePackageActionsFactory,
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base (consoleHost, terminatingError)
		{
//			this.updatePackageActionsFactory = updatePackageActionsFactory;
		}

		[Parameter (Position = 0, Mandatory = true, ParameterSetName = "Project")]
		[Parameter (Position = 0, ParameterSetName = "All")]
		public string Id { get; set; }

		[Parameter (Position = 1, ParameterSetName = "Project")]
		[Parameter (Position = 1, ParameterSetName = "All")]
		public string ProjectName { get; set; }

		[Parameter (Position = 2, ParameterSetName = "Project")]
		public SemanticVersion Version { get; set; }

		[Parameter (Position = 3)]
		public string Source { get; set; }

		[Parameter]
		public SwitchParameter IgnoreDependencies { get; set; }

		[Parameter, Alias ("Prerelease")]
		public SwitchParameter IncludePrerelease { get; set; }

		[Parameter]
		public FileConflictAction FileConflictAction { get; set; }

		[Parameter (Mandatory = true, ParameterSetName = "Reinstall")]
		[Parameter (ParameterSetName = "All")]
		public SwitchParameter Reinstall { get; set; }

//		protected override void ProcessRecord ()
//		{
//			ThrowErrorIfProjectNotOpen ();
//			using (IConsoleHostFileConflictResolver resolver = CreateFileConflictResolver ()) {
//				using (IDisposable monitor = CreateEventsMonitor ()) {
//					RunUpdate ();
//				}
//			}
//		}

		IConsoleHostFileConflictResolver CreateFileConflictResolver ()
		{
			return ConsoleHost.CreateFileConflictResolver (FileConflictAction);
		}

//		void RunUpdate()
//		{
//			if (HasPackageId ()) {
//				if (HasProjectName ()) {
//					if (Reinstall) {
//						ReinstallPackageInSingleProject ();
//					} else {
//						UpdatePackageInSingleProject ();
//					}
//				} else {
//					if (Reinstall) {
//						ReinstallPackageInAllProjects ();
//					} else {
//						UpdatePackageInAllProjects ();
//					}
//				}
//			} else {
//				if (HasProjectName()) {
//					if (Reinstall) {
//						ReinstallAllPackagesInProject ();
//					} else {
//						UpdateAllPackagesInProject ();
//					}
//				} else {
//					if (Reinstall) {
//						ReinstallAllPackagesInSolution ();
//					} else {
//						UpdateAllPackagesInSolution ();
//					}
//				}
//			}
//		}

		bool HasPackageId ()
		{
			return !String.IsNullOrEmpty (Id);
		}

		bool HasProjectName ()
		{
			return !String.IsNullOrEmpty (ProjectName);
		}

//		void UpdateAllPackagesInProject ()
//		{
//			IUpdatePackageActions2 actions = CreateUpdateAllPackagesInProject ();
//			RunActions (actions);
//		}
//
//		IUpdatePackageActions2 CreateUpdateAllPackagesInProject ()
//		{
//			IPackageManagementProject2 project = GetProject ();
//			return updatePackageActionsFactory.CreateUpdateAllPackagesInProject (project);
//		}
//
//		void UpdateAllPackagesInSolution ()
//		{
//			IUpdatePackageActions2 actions = CreateUpdateAllPackagesInSolution ();
//			RunActions (actions);
//		}
//
//		IUpdatePackageActions2 CreateUpdateAllPackagesInSolution ()
//		{
//			IPackageManagementSolution2 solution = ConsoleHost.Solution;
//			IPackageRepository repository = GetActivePackageRepository ();
//			return updatePackageActionsFactory.CreateUpdateAllPackagesInSolution (solution, repository);
//		}
//
//		IPackageRepository GetActivePackageRepository ()
//		{
//			PackageSource packageSource = ConsoleHost.GetActivePackageSource (Source);
//			return ConsoleHost.GetPackageRepository (packageSource);
//		}
//
//		void UpdatePackageInSingleProject ()
//		{
//			IPackageManagementProject2 project = GetProject ();
//			UpdatePackageAction2 action = CreateUpdatePackageAction (project);
//			using (IDisposable operation = StartUpdateOperation (action)) {
//				ExecuteWithScriptRunner (project, () => {
//					action.Execute ();
//				});
//			}
//		}
//
//		IDisposable StartUpdateOperation (UpdatePackageAction2 action)
//		{
//			return action.Project.SourceRepository.StartUpdateOperation (action.PackageId);
//		}
//
//		IPackageManagementProject2 GetProject ()
//		{
//			return ConsoleHost.GetProject (Source, ProjectName);
//		}
//
//		UpdatePackageAction2 CreateUpdatePackageAction (IPackageManagementProject2 project)
//		{
//			UpdatePackageAction2 action = project.CreateUpdatePackageAction ();
//			action.PackageId = Id;
//			action.PackageVersion = Version;
//			action.UpdateDependencies = UpdateDependencies;
//			action.AllowPrereleaseVersions = AllowPreleaseVersions;
////			action.PackageScriptRunner = this;
//			return action;
//		}
//
//		bool UpdateDependencies {
//			get { return !IgnoreDependencies.IsPresent; }
//		}
//
//		bool AllowPreleaseVersions {
//			get { return IncludePrerelease.IsPresent; }
//		}
//
//		void RunActions (IUpdatePackageActions2 updateActions)
//		{
//			updateActions.UpdateDependencies = UpdateDependencies;
//			updateActions.AllowPrereleaseVersions = AllowPreleaseVersions;
////			updateActions.PackageScriptRunner = this;
//			
//			foreach (UpdatePackageAction2 action in updateActions.CreateActions()) {
//				using (IDisposable operation = StartUpdateOperation (action)) {
//					ExecuteWithScriptRunner (action.Project, () => {
//						action.Execute ();
//					});
//				}
//			}
//		}
//
//		void UpdatePackageInAllProjects ()
//		{
//			IPackageManagementSolution2 solution = ConsoleHost.Solution;
//			IPackageRepository repository = GetActivePackageRepository ();
//			PackageReference packageReference = CreatePackageReference ();
//			IUpdatePackageActions2 updateActions =
//				updatePackageActionsFactory.CreateUpdatePackageInAllProjects (packageReference, solution, repository);
//			
//			RunActions (updateActions);
//		}
//
//		PackageReference CreatePackageReference ()
//		{
//			return new PackageReference (Id, Version, null, null, false, false);
//		}
//
//
//		void ReinstallPackageInSingleProject ()
//		{
//			IPackageManagementProject2 project = GetProject ();
//			IPackage package = FindPackageOrThrow (project);
//			ReinstallPackageInProject (project, package);
//		}
//
//		IPackage FindPackageOrThrow (IPackageManagementProject2 project)
//		{
//			IPackage package = project.FindPackage (Id, null);
//			if (package != null) {
//				return package;
//			}
//
//			throw CreatePackageNotFoundException (Id);
//		}
//
//		static InvalidOperationException CreatePackageNotFoundException (string packageId)
//		{
//			string message = String.Format ("Unable to find package '{0}'.", packageId);
//			throw new InvalidOperationException (message);
//		}
//
//		void ReinstallPackageInProject (IPackageManagementProject2 project, IPackage package)
//		{
//			ReinstallPackageAction2 action = CreateReinstallPackageAction (project, package);
//			using (IDisposable operation = StartReinstallOperation (action)) {
//				ExecuteWithScriptRunner (project, () => {
//					action.Execute ();
//				});
//			}
//		}
//
//		IDisposable StartReinstallOperation (ReinstallPackageAction2 action)
//		{
//			return action.Project.SourceRepository.StartReinstallOperation (action.PackageId);
//		}
//
//		ReinstallPackageAction2 CreateReinstallPackageAction (IPackageManagementProject2 project, IPackage package)
//		{
//			ReinstallPackageAction2 action = project.CreateReinstallPackageAction ();
//			action.PackageId = package.Id;
//			action.PackageVersion = package.Version;
//			action.UpdateDependencies = UpdateDependencies;
//			action.AllowPrereleaseVersions = AllowPreleaseVersions || !package.IsReleaseVersion ();
//
//			return action;
//		}
//
//		void ReinstallAllPackagesInProject ()
//		{
//			ReinstallAllPackagesInProject (GetProject ());
//		}
//
//		void ReinstallAllPackagesInProject (IPackageManagementProject2 project)
//		{
//			// No need to update dependencies since all packages will be reinstalled.
//			IgnoreDependencies = true;
//
//			foreach (IPackage package in project.GetPackages ()) {
//				ReinstallPackageInProject (project, package);
//			}
//		}
//
//		void ReinstallPackageInAllProjects ()
//		{
//			bool foundPackage = false;
//
//			IPackageRepository repository = GetActivePackageRepository ();
//			foreach (IPackageManagementProject2 project in ConsoleHost.Solution.GetProjects (repository)) {
//				IPackage package = project.FindPackage (Id, null);
//				if (package != null) {
//					foundPackage = true;
//					ReinstallPackageInProject (project, package);
//				}
//			}
//
//			if (!foundPackage) {
//				throw CreatePackageNotFoundException (Id);
//			}
//		}
//
//		void ReinstallAllPackagesInSolution ()
//		{
//			IPackageRepository repository = GetActivePackageRepository ();
//			foreach (IPackageManagementProject2 project in ConsoleHost.Solution.GetProjects (repository)) {
//				ReinstallAllPackagesInProject (project);
//			}
//		}
	}
}

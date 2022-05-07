//
// ConsoleHostSolutionManager.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;

namespace MonoDevelop.PackageManagement.Scripting
{
	class ConsoleHostSolutionManager : IConsoleHostSolutionManager, IMonoDevelopSolutionManager
	{
		MonoDevelopSolutionManager solutionManager;

		public ConsoleHostSolutionManager ()
		{
			IdeApp.Workspace.SolutionUnloaded += SolutionUnloaded;
			IdeApp.Workspace.ItemAddedToSolution += ProjectsChangedInSolution;
			IdeApp.Workspace.ItemRemovedFromSolution += ProjectsChangedInSolution;
		}

		void SolutionUnloaded (object sender, SolutionEventArgs e)
		{
			solutionManager = null;
		}

		void ProjectsChangedInSolution (object sender, EventArgs e)
		{
			solutionManager = null;
		}

		public NuGetProject DefaultNuGetProject {
			get {
				throw new NotImplementedException ();
			}
		}

		public string DefaultNuGetProjectName {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public Task<bool> IsSolutionAvailableAsync ()
		{
			throw new NotImplementedException ();
		}

		public bool IsSolutionOpen {
			get {
				return IdeApp.ProjectOperations.CurrentSelectedSolution != null;
			}
		}

		public INuGetProjectContext NuGetProjectContext { get; set; }

		public ISettings Settings {
			get {
				GetSolutionManager ();

				return solutionManager?.Settings;
			}
		}

		public string SolutionDirectory {
			get {
				GetSolutionManager ();

				return solutionManager?.SolutionDirectory;
			}
		}

		public Solution Solution {
			get {
				GetSolutionManager ();
				return solutionManager?.Solution;
			}
		}

		public ConfigurationSelector Configuration {
			get {
				GetSolutionManager ();
				return solutionManager?.Configuration;
			}
		}

		#pragma warning disable 67
		public event EventHandler<ActionsExecutedEventArgs> ActionsExecuted;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectAdded;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRemoved;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRenamed;
		public event EventHandler<NuGetProjectEventArgs> AfterNuGetProjectRenamed;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectUpdated;
		public event EventHandler<NuGetEventArgs<string>> AfterNuGetCacheUpdated;
		public event EventHandler SolutionClosed;
		public event EventHandler SolutionClosing;
		public event EventHandler SolutionOpened;
		public event EventHandler SolutionOpening;
		#pragma warning restore 67

		public ISourceRepositoryProvider CreateSourceRepositoryProvider ()
		{
			GetSolutionManager ();
			if (solutionManager != null)
				return solutionManager.CreateSourceRepositoryProvider ();

			var nullSolutionManager = new NullMonoDevelopSolutionManager ();
			return nullSolutionManager.CreateSourceRepositoryProvider ();
		}

		public NuGetProject GetNuGetProject (IDotNetProject project)
		{
			throw new NotImplementedException ();
		}

		public Task<global::EnvDTE.Project> GetEnvDTEProjectAsync (NuGetProject nuGetProject)
		{
			IDotNetProject dotNetProject = nuGetProject.GetDotNetProject ();
			if (dotNetProject != null) {
				var project = new MonoDevelop.PackageManagement.EnvDTE.Project (dotNetProject.DotNetProject);
				return Task.FromResult<global::EnvDTE.Project> (project);
			}
			return null;
		}

		public Task<IEnumerable<global::EnvDTE.Project>> GetAllEnvDTEProjectsAsync ()
		{
			GetSolutionManager ();

			if (solutionManager == null)
				return Task.FromResult (Enumerable.Empty<global::EnvDTE.Project> ());

			var projects = new List<global::EnvDTE.Project>();

			foreach (DotNetProject project in solutionManager.Solution.GetAllDotNetProjects ()) {
				projects.Add (new MonoDevelop.PackageManagement.EnvDTE.Project (project));
			}

			return Task.FromResult (projects.AsEnumerable ());
		}

		public Task<IEnumerable<NuGetProject>> GetNuGetProjectsAsync ()
		{
			GetSolutionManager ();

			if (solutionManager == null)
				return Task.FromResult (Enumerable.Empty<NuGetProject> ());

			return Task.FromResult (GetNuGetProjects (solutionManager).ToList ().AsEnumerable ());
		}

		static IEnumerable<NuGetProject> GetNuGetProjects (MonoDevelopSolutionManager solutionManager)
		{
			var factory = new ConsoleHostNuGetProjectFactory (solutionManager.Settings);
			foreach (DotNetProject project in solutionManager.Solution.GetAllDotNetProjects ()) {
				yield return factory.CreateNuGetProject (project);
			}
		}

		public async Task<string> GetNuGetProjectSafeNameAsync (NuGetProject nuGetProject)
		{
			if (nuGetProject == null) {
				throw new ArgumentNullException (nameof (nuGetProject));
			}

			GetSolutionManager ();

			// Try searching for simple names first
			string name = nuGetProject.GetMetadata<string> (NuGetProjectMetadataKeys.Name);
			var matchedNuGetProject = await GetNuGetProjectAsync (name);

			var matchedDotNetProject = matchedNuGetProject?.GetDotNetProject ()?.DotNetProject;
			if (matchedDotNetProject == nuGetProject.GetDotNetProject ().DotNetProject) {
				return name;
			}

			return nuGetProject.GetUniqueName ();
		}

		public void OnActionsExecuted (IEnumerable<ResolvedAction> actions)
		{
		}

		public void ReloadSettings ()
		{
		}

		public IMonoDevelopSolutionManager GetMonoDevelopSolutionManager ()
		{
			GetSolutionManager ();
			return solutionManager;
		}

		void GetSolutionManager ()
		{
			if (solutionManager != null) {
				var currentSolution = IdeApp.ProjectOperations.CurrentSelectedSolution;
				if (currentSolution != null) {
					if (currentSolution.FileName == solutionManager.Solution.FileName) {
						return;
					}
				}
			}

			var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
			if (solution != null) {
				Runtime.RunInMainThread (() => {
					solutionManager = PackageManagementServices.Workspace.GetSolutionManager (solution) as MonoDevelopSolutionManager;
				}).Wait ();
			} else {
				solutionManager = null;
			}
		}

		public void SaveProject (NuGetProject nuGetProject)
		{
		}

		public void ClearProjectCache ()
		{
		}

		public void EnsureSolutionIsLoaded ()
		{
		}

		public Task<bool> DoesNuGetSupportsAnyProjectAsync ()
		{
			throw new NotImplementedException ();
		}

		public string DefaultProjectFileName {
			get {
				Project project = PackageManagementExtendedServices.ConsoleHost.DefaultProject;
				return project?.FileName;
			}
		}

		public Task<IEnumerable<NuGetProject>> GetAllNuGetProjectsAsync ()
		{
			return GetNuGetProjectsAsync ();
		}

		public Task<NuGetProject> GetDefaultNuGetProjectAsync ()
		{
			var project = PackageManagementExtendedServices.ConsoleHost.DefaultProject as DotNetProject;
			if (project != null) {
				var factory = new ConsoleHostNuGetProjectFactory ();
				NuGetProject nuGetProject = factory.CreateNuGetProject (project);
				return Task.FromResult (nuGetProject);
			}
			return null;
		}

		public async Task<NuGetProject> GetNuGetProjectAsync (string projectName)
		{
			if (string.IsNullOrEmpty (projectName)) {
				return null;
			}

			var projects = await GetAllNuGetProjectsAsync ();
			return projects.FirstOrDefault (project => IsMatch (project, projectName));
		}

		static bool IsMatch (NuGetProject project, string projectName)
		{
			DotNetProject dotNetProject = project.GetDotNetProject ()?.DotNetProject;
			if (dotNetProject == null) {
				return false;
			}

			return StringComparer.OrdinalIgnoreCase.Equals (dotNetProject.Name, projectName) ||
				StringComparer.OrdinalIgnoreCase.Equals (
					dotNetProject.GetUniqueName (),
					projectName);
		}
	}
}

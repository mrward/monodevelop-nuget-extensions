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

namespace MonoDevelop.PackageManagement
{
	class ConsoleHostSolutionManager : IMonoDevelopSolutionManager
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

		public bool IsSolutionAvailable {
			get {
				throw new NotImplementedException ();
			}
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

		public NuGetProject GetNuGetProject (string nuGetProjectSafeName)
		{
			GetSolutionManager ();

			NuGetProject project = null;

			Runtime.RunInMainThread (() => {
				var dotNetProject = IdeApp.ProjectOperations.CurrentSelectedSolution?.FindProjectByName (nuGetProjectSafeName) as DotNetProject;
				if (dotNetProject != null) {
					var factory = new ConsoleHostNuGetProjectFactory (solutionManager.Settings);
					project = factory.CreateNuGetProject (dotNetProject);
				}
			}).Wait ();

			return project;
		}

		public IEnumerable<NuGetProject> GetNuGetProjects ()
		{
			GetSolutionManager ();

			if (solutionManager == null)
				return Enumerable.Empty<NuGetProject> ();
			
			List<NuGetProject> projects = null;

			Runtime.RunInMainThread (() => {
				projects = GetNuGetProjects (solutionManager).ToList ();
			}).Wait ();

			return projects;
		}

		static IEnumerable<NuGetProject> GetNuGetProjects (MonoDevelopSolutionManager solutionManager)
		{
			var factory = new ConsoleHostNuGetProjectFactory (solutionManager.Settings);
			foreach (DotNetProject project in solutionManager.Solution.GetAllDotNetProjects ()) {
				yield return factory.CreateNuGetProject (project);
			}
		}

		public string GetNuGetProjectSafeName (NuGetProject nuGetProject)
		{
			throw new NotImplementedException ();
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

		public bool IsSolutionDPLEnabled { get; set; }

		public void EnsureSolutionIsLoaded ()
		{
		}

		public Task<NuGetProject> UpdateNuGetProjectToPackageRef (NuGetProject oldProject)
		{
			return Task.FromResult (oldProject);
		}
	}
}

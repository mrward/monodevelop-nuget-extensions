// 
// Solution.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2012-2014 Matthew Ward
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
using System.Collections;
using System.Collections.Generic;
using MD = MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	public class Solution : MonoDevelop.EnvDTE.SolutionBase, global::EnvDTE.Solution
	{
		IExtendedPackageManagementProjectService projectService;
		MD.Solution solution;

		public Solution (IExtendedPackageManagementProjectService projectService)
		{
			this.projectService = projectService;
			this.solution = projectService.OpenSolution;
			this.Projects = new Projects (projectService);
//			this.Globals = new SolutionGlobals (this);
			this.SolutionBuild = new SolutionBuild (this);
			CreateProperties ();
		}

		void CreateProperties ()
		{
			var propertyFactory = new SolutionPropertyFactory (this);
			Properties = new Properties (propertyFactory);
		}

		internal MD.Solution MonoDevelopSolution => solution;

		public string FullName {
			get { return FileName; }
		}

		public string FileName {
			get { return solution.FileName; }
		}

		public bool IsOpen {
			get { return projectService.OpenSolution == solution; }
		}

		public global::EnvDTE.Projects Projects { get; private set; }

		public global::EnvDTE.Globals Globals { get; private set; }
//
//		internal ICollection<SD.SolutionSection> Sections {
//			get { return solution.GlobalSections; }
//		}

		internal void Save ()
		{
			Runtime.RunInMainThread (() => {
				solution.SaveAsync (new ProgressMonitor ());
			}).Wait ();
		}

		public global::EnvDTE.ProjectItem FindProjectItem (string fileName)
		{
//			foreach (Project project in Projects) {
//				ProjectItem item = project.FindProjectItem (fileName);
//				if (item != null) {
//					return item;
//				}
//			}
			return null;
		}

		public global::EnvDTE.SolutionBuild SolutionBuild { get; private set; }

		public global::EnvDTE.Properties Properties { get; private set; }

		internal Project GetStartupProject ()
		{
			var project = solution.StartupItem as MD.DotNetProject;
			if (project != null) {
				return new Project (project);
			}
			return null;
		}

		internal IEnumerable<Project> GetStartupProjects ()
		{
			if (solution.SingleStartup) {
				yield return GetStartupProject ();
			} else {
				foreach (MD.SolutionItem solutionItem in solution.MultiStartupItems) {
					var project = solutionItem as MD.DotNetProject;
					if (project != null) {
						yield return new Project (project);
					}
				}
			}
		}

		internal IEnumerable<string> GetAllPropertyNames ()
		{
			return new string[] {
				"Path",
				"StartupProject"
			};
		}

		//		internal SolutionConfiguration GetActiveConfiguration ()
		//		{
		//			return new SolutionConfiguration (solution);
		//		}

		public global::EnvDTE.Project Item (object index)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public void SaveAs (string fileName)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Project AddFromTemplate (string fileName, string destination, string projectName, bool exclusive = false)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Project AddFromFile (string fileName, bool exclusive = false)
		{
			throw new NotImplementedException ();
		}

		public void Open (string fileName)
		{
			throw new NotImplementedException ();
		}

		public void Close (bool saveFirst = false)
		{
			throw new NotImplementedException ();
		}

		public void Remove (global::EnvDTE.Project project)
		{
			throw new NotImplementedException ();
		}

		public void Create (string destination, string name)
		{
			throw new NotImplementedException ();
		}

		public string ProjectItemsTemplatePath (string projectKind)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.DTE DTE => throw new NotImplementedException ();

		public global::EnvDTE.DTE Parent => throw new NotImplementedException ();

		public int Count => throw new NotImplementedException ();

		public bool IsDirty { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		protected override string GetTemplatePath(string projectType)
		{
			return null;
		}

		public bool Saved { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.AddIns AddIns => throw new NotImplementedException ();

		protected override object GetExtender(string extenderName)
		{
			return null;
		}

		public object ExtenderNames => throw new NotImplementedException ();

		public string ExtenderCATID => throw new NotImplementedException ();
	}
}

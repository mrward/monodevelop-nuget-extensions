//
// ExtendedProjectService.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
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
using MonoDevelop.Projects;
using MonoDevelop.Ide;

namespace MonoDevelop.PackageManagement
{
	public class ExtendedPackageManagementProjectService : IExtendedPackageManagementProjectService
	{
		public ExtendedPackageManagementProjectService ()
		{
			IdeApp.Workspace.SolutionLoaded += (sender, e) => OnSolutionLoaded (e.Solution);
			IdeApp.Workspace.SolutionUnloaded += (sender, e) => OnSolutionUnloaded ();
		}

		public event EventHandler SolutionLoaded;

		void OnSolutionLoaded (Solution solution)
		{
			OpenSolution = solution;

			EventHandler handler = SolutionLoaded;
			if (handler != null) {
				handler (this, new EventArgs ());
			}
		}

		public event EventHandler SolutionUnloaded;

		void OnSolutionUnloaded ()
		{
			OpenSolution = null;

			var handler = SolutionUnloaded;
			if (handler != null) {
				handler (this, new EventArgs ());
			}
		}

		public DotNetProject CurrentProject {
			get {
				return IdeApp.ProjectOperations.CurrentSelectedProject as DotNetProject;
			}
		}

		public Solution OpenSolution { get; private set; }

		public IEnumerable<DotNetProject> GetOpenProjects ()
		{
			if (OpenSolution != null) {
				return OpenSolution.GetAllProjects ().OfType<DotNetProject> ();
			}
			return new DotNetProject [0];
		}

		public string GetDefaultCustomToolForFileName(ProjectFile projectItem)
		{
			return String.Empty;
		}
	}
}


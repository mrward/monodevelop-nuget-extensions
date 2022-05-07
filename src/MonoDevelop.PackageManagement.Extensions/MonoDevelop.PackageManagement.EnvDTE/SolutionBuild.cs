//
// SolutionBuild.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2022 Microsoft
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide;

using MD = MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	public class SolutionBuild : global::EnvDTE.SolutionBuild
	{
		readonly Solution solution;
		string[] startupProjects;

		internal SolutionBuild (Solution solution)
		{
			this.solution = solution;
		}

		public void Build (bool waitForBuildToFinish = false)
		{
			LastBuildInfo = 0;

			Task<MD.BuildResult> task = Runtime.RunInMainThread (() => {
				return IdeApp.ProjectOperations.Build (solution.MonoDevelopSolution).Task;
			});

			if (waitForBuildToFinish) {
				Runtime.JoinableTaskFactory.Run (async () => {
					MD.BuildResult result = await task;
					LastBuildInfo = result.FailedBuildCount;
				});
			}
		}

		public void Debug ()
		{
			throw new NotImplementedException ();
		}

		public void Deploy (bool WaitForDeployToFinish = false)
		{
			throw new NotImplementedException ();
		}

		public void Clean (bool WaitForCleanToFinish = false)
		{
			throw new NotImplementedException ();
		}

		public void Run ()
		{
			throw new NotImplementedException ();
		}

		public void BuildProject (string solutionConfiguration, string projectUniqueName, bool waitForBuildToFinish = false)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.DTE DTE => throw new NotImplementedException ();

		public global::EnvDTE.Solution Parent => solution;

		public global::EnvDTE.SolutionConfiguration ActiveConfiguration => throw new NotImplementedException ();

		public global::EnvDTE.BuildDependencies BuildDependencies => throw new NotImplementedException ();

		public global::EnvDTE.vsBuildState BuildState => throw new NotImplementedException ();

		/// <summary>
		/// Returns the number of projects that failed to build.
		/// </summary>
		public int LastBuildInfo { get; private set; }

		public object StartupProjects {
			get {
				if (startupProjects != null) {
					return startupProjects;
				}

				startupProjects = GetStartupProjectUniqueNames ().ToArray ();
				return startupProjects;
			}

			set { throw new NotImplementedException (); }
		}

		public global::EnvDTE.SolutionConfigurations SolutionConfigurations => throw new NotImplementedException ();

		IEnumerable<string> GetStartupProjectUniqueNames ()
		{
			foreach (Project project in solution.GetStartupProjects ()) {
				yield return project.UniqueName;
			}
		}
	}
}


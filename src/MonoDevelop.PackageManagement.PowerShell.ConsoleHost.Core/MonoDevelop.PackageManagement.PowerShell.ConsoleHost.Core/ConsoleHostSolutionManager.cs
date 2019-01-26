//
// ConsoleHostSolutionManager.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft
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
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core
{
	public class ConsoleHostSolutionManager : IConsoleHostSolutionManager
	{
		string solutionFileName;

		public bool IsSolutionOpen {
			get { return solutionFileName != null; }
		}

		public string DefaultProjectName { get; set; }

		public void OnSolutionLoaded (string fileName)
		{
			solutionFileName = fileName;
		}

		public void OnSolutionUnloaded ()
		{
			solutionFileName = null;
		}

		public Task<global::EnvDTE.Project> GetDefaultProjectAsync ()
		{
			return GetProjectAsync (DefaultProjectName);
		}

		public Task<IEnumerable<global::EnvDTE.Project>> GetAllProjectsAsync ()
		{
			var dte = new DTE ();
			var projects = new List<global::EnvDTE.Project> ();
			foreach (var item in dte.Solution.Projects) {
				if (item is global::EnvDTE.Project project) {
					projects.Add (project);
				}
			}
			return Task.FromResult (projects.AsEnumerable ());
		}

		public async Task<global::EnvDTE.Project> GetProjectAsync (string projectName)
		{
			if (string.IsNullOrEmpty (projectName)) {
				return null;
			}

			var projects = await GetAllProjectsAsync ();
			return projects.FirstOrDefault (project => IsMatch (project, projectName));
		}

		static bool IsMatch (global::EnvDTE.Project project, string projectName)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (project.Name, projectName);
		}
	}
}

//
// Projects.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using MonoDevelop.PackageManagement.PowerShell.Protocol;

namespace MonoDevelop.PackageManagement.PowerShell.EnvDTE
{
	public class Projects : MarshalByRefObject, IEnumerable<Project>, global::EnvDTE.Projects
	{
		Solution solution;

		public Projects (Solution solution)
		{
			this.solution = solution;
		}

		public IEnumerator<Project> GetEnumerator ()
		{
			return GetProjectsInSolution ().GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		IEnumerable<Project> GetProjectsInSolution ()
		{
			foreach (var projectInfo in GetProjectInformation ()) {
				yield return new Project (projectInfo);
			}
		}

		ProjectInformation[] GetProjectInformation ()
		{
			var message = new ProjectInformationParams {
				SolutionFileName = solution.FileName
			};
			var list = JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<ProjectInformationList> (Methods.SolutionProjects, message).Result;
			return list.Projects;
		}

		public int Count {
			get { return GetProjectsInSolution ().Count (); }
		}

		/// <summary>
		/// Index of 1 returns the first project.
		/// </summary>
		public global::EnvDTE.Project Item (object index)
		{
			if (index is int) {
				return Item ((int)index);
			}
			return Item ((string)index);
		}

		global::EnvDTE.Project Item (int index)
		{
			return GetProjectsInSolution ()
				.Skip (index - 1)
				.First ();
		}

		global::EnvDTE.Project Item (string uniqueName)
		{
			return GetProjectsInSolution ()
				.First (p => p.UniqueName == uniqueName);
		}
	}
}

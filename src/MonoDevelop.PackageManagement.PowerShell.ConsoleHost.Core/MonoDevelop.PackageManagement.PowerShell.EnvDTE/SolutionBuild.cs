//
// SolutionBuild.cs
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
using EnvDTE;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using MonoDevelop.PackageManagement.PowerShell.Protocol;

namespace MonoDevelop.PackageManagement.PowerShell.EnvDTE
{
	public class SolutionBuild : MarshalByRefObject, global::EnvDTE.SolutionBuild
	{
		readonly Solution solution;
		string[] startupProjects;

		public SolutionBuild (Solution solution)
		{
			this.solution = solution;
		}

		public SolutionConfiguration ActiveConfiguration { get; private set; }

		public object StartupProjects {
			get {
				if (startupProjects != null) {
					return startupProjects;
				}

				startupProjects = GetStartupProjectUniqueNames ().ToArray ();
				return startupProjects;
			}
		}

		/// <summary>
		/// Returns the number of projects that failed to build.
		/// </summary>
		public int LastBuildInfo { get; private set; }

		public void BuildProject (
			string solutionConfiguration,
			string projectUniqueName,
			bool waitForBuildToFinish = false)
		{
		}

		public void Build (bool waitForBuildToFinish = false)
		{
			var message = new ProjectInformationParams {
				SolutionFileName = solution.FileName
			};

			if (waitForBuildToFinish) {
				var result = JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<BuildResultInformation> (
					Methods.BuildSolutionName,
					message).WaitAndGetResult ();
				LastBuildInfo = result.ProjectBuildFailureCount;
			} else {
				throw new NotImplementedException ();
			}
		}

		IEnumerable<string> GetStartupProjectUniqueNames ()
		{
			var message = new ProjectInformationParams {
				SolutionFileName = solution.FileName
			};
			var list = JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<ProjectInformationList> (
				Methods.StartupProjectsName,
				message).WaitAndGetResult ();
			return list.Projects.Select (project => project.UniqueName);
		}
	}
}

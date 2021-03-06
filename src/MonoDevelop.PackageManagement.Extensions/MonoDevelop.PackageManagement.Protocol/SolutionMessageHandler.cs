﻿//
// SolutionMessageHandler.cs
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
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.Protocol
{
	class SolutionMessageHandler
	{
		[JsonRpcMethod (Methods.SolutionProjects)]
		public ProjectInformationList OnGetSolutionProjects (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectInformationParams> ();
				var list = new ProjectInformationList {
					Projects = GetProjectsInSolution ().ToArray ()
				};
				return list;
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetSolutionProjects error", ex);
				throw;
			}
		}

		IEnumerable<ProjectInformation> GetProjectsInSolution ()
		{
			foreach (IDotNetProject project in GetOpenMSBuildProjects ()) {
				yield return project.CreateProjectInformation ();
			}
		}

		IEnumerable<IDotNetProject> GetOpenMSBuildProjects ()
		{
			return PackageManagementServices.ProjectService.GetOpenProjects ();
		}

		[JsonRpcMethod (Methods.StartupProjectsName)]
		public ProjectInformationList OnGetStartupProjects (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectInformationParams> ();
				var startupProject = PackageManagementServices
					.ProjectService
					.OpenSolution
					.Solution
					.StartupItem as DotNetProject;

				var list = new ProjectInformationList ();
				if (startupProject != null) {
					list.Projects = new [] { startupProject.CreateProjectInformation () };
				} else {
					list.Projects = Array.Empty<ProjectInformation> ();
				}
				return list;
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetStartupProjects error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.BuildSolutionName)]
		public BuildResultInformation OnBuildSolution (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectInformationParams> ();
				var solution = PackageManagementServices.ProjectService.OpenSolution;

				Task<BuildResult> task = Runtime.RunInMainThread (() => {
					return IdeApp.ProjectOperations.Build (solution.Solution).Task;
				});
				BuildResult result = task.WaitAndGetResult ();

				return new BuildResultInformation {
					ProjectBuildFailureCount = result.FailedBuildCount
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnBuildSolution error", ex);
				throw;
			}
		}
	}
}

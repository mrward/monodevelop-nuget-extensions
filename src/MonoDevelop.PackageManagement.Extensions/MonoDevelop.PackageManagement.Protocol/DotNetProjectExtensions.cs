//
// DotNetProjectExtensions.cs
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

using ICSharpCode.PackageManagement.EnvDTE;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;
using NuGet.ProjectManagement;

namespace MonoDevelop.PackageManagement.Protocol
{
	static class DotNetProjectExtensions
	{
		public static IMonoDevelopSolutionManager GetSolutionManager (this DotNetProject project)
		{
			return Runtime.RunInMainThread (() => {
				return PackageManagementServices.Workspace.GetSolutionManager (project.ParentSolution);
			}).WaitAndGetResult ();
		}

		public static NuGetProject CreateNuGetProject (this DotNetProject project, IMonoDevelopSolutionManager solutionManager)
		{
			if (solutionManager != null) {
				var nugetProject = solutionManager.GetNuGetProject (new DotNetProjectProxy (project))
					.WithConsoleHostProjectServices (project);
			}

			return new MonoDevelopNuGetProjectFactory ()
				.CreateNuGetProject (project)
				.WithConsoleHostProjectServices (project);
		}

		public static ProjectInformation CreateProjectInformation (this IDotNetProject project)
		{
			return CreateProjectInformation (project.DotNetProject);
		}

		public static ProjectInformation CreateProjectInformation (this DotNetProject project)
		{
			return new ProjectInformation {
				Name = project.Name,
				FileName = project.FileName,
				UniqueName = GetUniqueName (project),
				Kind = ProjectKind.GetProjectKind (project),
				TargetFrameworkMoniker = project.TargetFramework.Id.ToString (),
				Type = ProjectType.GetProjectType (project)
			};
		}

		static string GetUniqueName (DotNetProject project)
		{
			return FileService.AbsoluteToRelativePath (
				project.ParentSolution.BaseDirectory,
				project.FileName);
		}
	}
}

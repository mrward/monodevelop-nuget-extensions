//
// GlobalMSBuildProjectCollection.cs
//
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
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

using Microsoft.Build.Evaluation;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement;
using MonoDevelop.PackageManagement.Scripting;
using DotNetProject = MonoDevelop.Projects.DotNetProject;
using NuGet;

namespace ICSharpCode.PackageManagement.Scripting
{
	internal class GlobalMSBuildProjectCollection : IGlobalMSBuildProjectCollection
	{
		class  GlobalAndInternalProject
		{
			public Project GlobalMSBuildProject;
			public DotNetProject DotNetProject;
			public int GlobalMSBuildProjectImportsCount;

			public bool HasGlobalMSBuildProjectImportsChanged ()
			{
				return GlobalMSBuildProjectImportsCount != GlobalMSBuildProject.Xml.Imports.Count;
			}
		}

		List<GlobalAndInternalProject> projects = new List<GlobalAndInternalProject> ();

		PackageManagementLogger logger = new PackageManagementLogger (
			new ThreadSafePackageManagementEvents (PackageManagementServices.PackageManagementEvents));

		public void AddProject (IPackageManagementProject2 packageManagementProject)
		{
			AddProject (packageManagementProject.DotNetProject);
		}

		void AddProject (DotNetProject dotNetProject)
		{
			Project globalProject = GetGlobalProjectCollection ().LoadProject (dotNetProject.FileName);

			projects.Add (new GlobalAndInternalProject {
				GlobalMSBuildProject = globalProject,
				DotNetProject = dotNetProject,
				GlobalMSBuildProjectImportsCount = globalProject.Xml.Imports.Count
			});
		}

		ProjectCollection GetGlobalProjectCollection ()
		{
			return ProjectCollection.GlobalProjectCollection;
		}

		public void Dispose ()
		{
			foreach (GlobalAndInternalProject msbuildProjects in projects) {
				Runtime.RunInMainThread (() => UpdateProject (msbuildProjects)).Wait ();
				GetGlobalProjectCollection ().UnloadProject (msbuildProjects.GlobalMSBuildProject);
			}
		}

		void UpdateProject (GlobalAndInternalProject msbuildProjects)
		{
			UpdateImports (msbuildProjects);
			UpdateProperties (msbuildProjects);
		}

		void UpdateImports (GlobalAndInternalProject msbuildProjects)
		{
			if (!msbuildProjects.HasGlobalMSBuildProjectImportsChanged ()) {
				return;
			}

			LogProjectImportsChanged (msbuildProjects.DotNetProject);

			var importsMerger = new MSBuildProjectImportsMerger (
				msbuildProjects.GlobalMSBuildProject,
				msbuildProjects.DotNetProject);

			GlobalMSBuildProjectCollectionMSBuildExtension.ImportsMerger = importsMerger;
			msbuildProjects.DotNetProject.SaveAsync (new ProgressMonitor ()).Wait ();

			LogProjectImportMergeResult (msbuildProjects.DotNetProject, importsMerger.Result);
		}

		void LogProjectImportsChanged (DotNetProject project)
		{
			logger.Log (
				MessageLevel.Info,
				"Project imports have been modified outside {0} for project '{1}'.",
				BrandingService.ApplicationName,
				project.Name);
		}

		void LogProjectImportMergeResult (DotNetProject project, MSBuildProjectImportsMergeResult result)
		{
			logger.Log (
				MessageLevel.Info,
				"Project import merge result for project '{0}':\r\n{1}",
				project.Name,
				result);
		}

		void UpdateProperties (GlobalAndInternalProject msbuildProjects)
		{
			var propertiesMerger = new MSBuildProjectPropertiesMerger (
				msbuildProjects.GlobalMSBuildProject,
				msbuildProjects.DotNetProject);

			propertiesMerger.Merge ();

			if (propertiesMerger.Result.AnyPropertiesChanged ()) {
				LogProjectPropertiesMergeResult (msbuildProjects.DotNetProject, propertiesMerger.Result);
			}
		}

		void LogProjectPropertiesMergeResult (DotNetProject project, MSBuildProjectPropertiesMergeResult result)
		{
			logger.Log (
				MessageLevel.Info,
				"Project properties merge result for project '{0}':\r\n{1}",
				project.Name,
				result);
		}
	}
}

//
// MSBuildProjectImportsMerger.cs
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
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using MonoDevelop.PackageManagement;
using MonoDevelop.Projects.MSBuild;
using NuGet;
using DotNetProject = MonoDevelop.Projects.DotNetProject;

namespace MonoDevelop.PackageManagement.Scripting
{
	internal class MSBuildProjectImportsMerger
	{
		IPackageManagementProjectService projectService;
		Project msbuildProject;
		DotNetProject dotNetProject;
		MSBuildProject originalMSBuildProject;
		MSBuildProjectImportsMergeResult result = new MSBuildProjectImportsMergeResult ();

		public MSBuildProjectImportsMerger (Project msbuildProject, DotNetProject dotNetProject)
			: this (msbuildProject, dotNetProject, new PackageManagementProjectService ())
		{
		}

		public MSBuildProjectImportsMerger (
			Project msbuildProject,
			DotNetProject dotNetProject,
			IPackageManagementProjectService projectService)
		{
			this.msbuildProject = msbuildProject;
			this.dotNetProject = dotNetProject;
			this.projectService = projectService;
		}

		public MSBuildProjectImportsMergeResult Result {
			get { return result; }
		}

		public void Merge (MSBuildProject originalMSBuildProject)
		{
			this.originalMSBuildProject = originalMSBuildProject;
			int msbuildProjectImportCount = msbuildProject.Xml.Imports.Count;
			int dotProjectImportCount = originalMSBuildProject.Imports.Count ();
			if (msbuildProjectImportCount > dotProjectImportCount) {
				AddNewImports ();
			} else if (msbuildProjectImportCount < dotProjectImportCount) {
				RemoveMissingImports ();
			}
		}

		void RemoveMissingImports ()
		{
			var importsToRemove = new List<MSBuildImport> ();
			foreach (MSBuildImport import in originalMSBuildProject.Imports) {
				if (msbuildProject.Xml.FindImport (import.Project) == null) {
					importsToRemove.Add (import);
				}
			}

			foreach (MSBuildImport importToRemove in importsToRemove) {
				importToRemove.ParentProject.RemoveImport (importToRemove);
			}

			result.AddProjectImportsRemoved (importsToRemove);
		}

		void AddNewImports ()
		{
			var importsToAdd = new List<ProjectImportElement> ();
			foreach (ProjectImportElement import in msbuildProject.Xml.Imports) {
				if (!originalMSBuildProject.ImportExists (import.Project)) {
					importsToAdd.Add (import);
				}
			}

			foreach (ProjectImportElement importToAdd in importsToAdd) {
				string condition = GetCondition (importToAdd.Project);
//				originalMSBuildProject.AddImport (importToAdd.Project, ImportLocation.Bottom, condition);
			}

			result.AddProjectImportsAdded (importsToAdd);
		}

		static string GetCondition (string targetPath)
		{
			return String.Format ("Exists('{0}')", targetPath);
		}
	}
}

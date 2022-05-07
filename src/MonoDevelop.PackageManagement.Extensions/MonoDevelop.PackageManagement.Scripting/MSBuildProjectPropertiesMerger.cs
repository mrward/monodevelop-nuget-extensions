//
// MSBuildProjectPropertiesMerger.cs
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
using MonoDevelop.Core;
using MonoDevelop.PackageManagement;
using DotNetProject = MonoDevelop.Projects.DotNetProject;

namespace MonoDevelop.PackageManagement.Scripting
{
	internal class MSBuildProjectPropertiesMerger
	{
		IPackageManagementProjectService projectService;
		Project msbuildProject;
		DotNetProject dotNetProject;
		MSBuildProjectPropertiesMergeResult result = new MSBuildProjectPropertiesMergeResult ();

		public MSBuildProjectPropertiesMerger (Project msbuildProject, DotNetProject dotNetProject)
			: this (msbuildProject, dotNetProject, new PackageManagementProjectService ())
		{
		}

		public MSBuildProjectPropertiesMerger (
			Project msbuildProject,
			DotNetProject dotNetProject,
			IPackageManagementProjectService projectService)
		{
			this.msbuildProject = msbuildProject;
			this.dotNetProject = dotNetProject;
			this.projectService = projectService;
		}

		public MSBuildProjectPropertiesMergeResult Result {
			get { return result; }
		}

		public void Merge ()
		{
			foreach (ProjectPropertyElement property in msbuildProject.Xml.Properties) {
				UpdateProperty (property);
			}

			//if (result.AnyPropertiesChanged ()) {
				dotNetProject.SaveAsync (new ProgressMonitor ());
			//}
		}

		void UpdateProperty (ProjectPropertyElement msbuildProjectProperty)
		{
			return;

			List<ProjectPropertyElement> dotNetProjectProperties = FindDotNetProjectProperties (msbuildProjectProperty);
			if (dotNetProjectProperties.Count > 1) {
				// Ignore. Currently do not handle properties defined inside
				// property groups with conditions (e.g. OutputPath)
			} else if (!dotNetProjectProperties.Any ()) {
				AddPropertyToDotNetProject (msbuildProjectProperty);
			} else if (HasMSBuildProjectPropertyBeenUpdated (msbuildProjectProperty, dotNetProjectProperties.First ())) {
				UpdatePropertyInDotNetProject (msbuildProjectProperty);
			}
		}

		List<ProjectPropertyElement> FindDotNetProjectProperties (ProjectPropertyElement msbuildProjectProperty)
		{
			return new List <ProjectPropertyElement> ();
//			lock (dotNetProject.SyncRoot) {
//				return dotNetProject
//					.MSBuildProjectFile
//					.Properties
//					.Where (property => String.Equals (property.Name, msbuildProjectProperty.Name, StringComparison.OrdinalIgnoreCase))
//					.ToList ();
//			}
		}

		void AddPropertyToDotNetProject (ProjectPropertyElement msbuildProjectProperty)
		{
			SetPropertyInDotNetProject (msbuildProjectProperty);
			result.AddPropertyAdded (msbuildProjectProperty.Name);
		}

		void SetPropertyInDotNetProject (ProjectPropertyElement msbuildProjectProperty)
		{
			throw new NotImplementedException();
			//dotNetProject.SetProperty (msbuildProjectProperty.Name, msbuildProjectProperty.Value);
		}

		bool HasMSBuildProjectPropertyBeenUpdated (ProjectPropertyElement msbuildProjectProperty, ProjectPropertyElement dotNetProjectProperty)
		{
			return msbuildProjectProperty.Value != dotNetProjectProperty.Value;
		}

		void UpdatePropertyInDotNetProject (ProjectPropertyElement msbuildProjectProperty)
		{
			SetPropertyInDotNetProject (msbuildProjectProperty);
			result.AddPropertyUpdated (msbuildProjectProperty.Name);
		}
	}
}

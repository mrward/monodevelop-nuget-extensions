// 
// ProjectItemsInsideProject.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2012-2014 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MD = MonoDevelop.Projects;
using MonoDevelop.Core;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class ProjectItemsInsideProject : EnumerableProjectItems
	{
		Project project;
		Dictionary<string, string> directoriesIncluded = new Dictionary<string, string> ();
		IPackageManagementFileService fileService;

		public ProjectItemsInsideProject (Project project, IPackageManagementFileService fileService)
		{
			this.project = project;
			this.fileService = fileService;
		}

		protected override IEnumerable<global::EnvDTE.ProjectItem> GetProjectItems ()
		{
			foreach (MD.ProjectItem item in project.DotNetProject.Items) {
				ProjectItem projectItem = ConvertToProjectItem (item);
				if (projectItem != null) {
					yield return projectItem;
				}
			}
		}

		ProjectItem ConvertToProjectItem (MD.ProjectItem item)
		{
			var fileItem = item as MD.ProjectFile;
			if ((fileItem != null) && String.IsNullOrEmpty (fileItem.DependsOn)) {
				return ConvertFileToProjectItem (fileItem);
			}
			return null;
		}

		ProjectItem ConvertFileToProjectItem (MD.ProjectFile fileItem)
		{
			if (IsInProjectRootFolder (fileItem)) {
				if (IsDirectory (fileItem)) {
					return CreateDirectoryProjectItemIfDirectoryNotAlreadyIncluded (fileItem);
				}
				return new ProjectItem (project, fileItem);
			}
			return ConvertDirectoryToProjectItem (fileItem);
		}

		bool IsInProjectRootFolder (MD.ProjectFile item)
		{
			if (item.IsLink) {
				return !HasDirectoryInPath (item.Link);
			}
			return !HasDirectoryInPath (item.FilePath);
		}

		bool HasDirectoryInPath (string path)
		{
			string relativePath = project.GetRelativePath (path);
			string directoryName = Path.GetDirectoryName (relativePath);
			return !String.IsNullOrEmpty (directoryName);
		}

		bool IsDirectory (MD.ProjectFile fileItem)
		{
			return fileItem.FilePath.IsDirectory;
		}

		ProjectItem CreateDirectoryProjectItemIfDirectoryNotAlreadyIncluded (MD.ProjectFile fileItem)
		{
			string directory = fileItem.FilePath;
			if (!IsDirectoryIncludedAlready (directory)) {
				AddIncludedDirectory (directory);
				return new ProjectItem (project, fileItem);
			}
			return null;
		}

		ProjectItem ConvertDirectoryToProjectItem (MD.ProjectFile fileItem)
		{
			string relativePath = project.GetRelativePath (fileItem.FilePath);
			string subDirectoryName = GetFirstSubDirectoryName (relativePath);
			if (IsDirectoryInsideProject (subDirectoryName)) {
				string fullPath = project.DotNetProject.BaseDirectory.Combine (subDirectoryName);
				return CreateDirectoryProjectItemIfDirectoryNotAlreadyIncluded (subDirectoryName, fullPath);
			}
			return null;
		}

		ProjectItem CreateDirectoryProjectItemIfDirectoryNotAlreadyIncluded (string subDirectoryName, string fullPath)
		{
			if (!IsDirectoryIncludedAlready (subDirectoryName)) {
				AddIncludedDirectory (subDirectoryName);
				return CreateDirectoryProjectItem (subDirectoryName, fullPath);
			}
			return null;
		}

		bool IsDirectoryInsideProject (string directoryName)
		{
			return !directoryName.StartsWith ("..");
		}

		bool IsDirectoryIncludedAlready (string directory)
		{
			return directoriesIncluded.ContainsKey (directory);
		}

		void AddIncludedDirectory (string directoryName)
		{
			directoriesIncluded.Add (directoryName, directoryName);
		}

		ProjectItem CreateDirectoryProjectItem (string directoryName, string fullPath)
		{
			var directoryItem = new MD.ProjectFile (fullPath);
			return new ProjectItem (project, directoryItem) { Kind = global::EnvDTE.Constants.vsProjectItemKindPhysicalFolder };
		}

		string GetFirstSubDirectoryName (string include)
		{
			string[] directoryNames = include.Split ('\\');
			return directoryNames [0];
		}
	}
}

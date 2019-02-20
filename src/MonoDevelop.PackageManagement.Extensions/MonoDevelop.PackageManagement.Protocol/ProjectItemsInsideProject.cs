//
// ProjectItemsInsideProject.cs
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
using System.IO;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.Protocol
{
	class ProjectItemsInsideProject
	{
		readonly Project project;
		Dictionary<string, string> directoriesIncluded = new Dictionary<string, string> ();

		public ProjectItemsInsideProject (Project project)
		{
			this.project = project;
		}

		public IEnumerable<ProjectItemInformation> GetProjectItems ()
		{
			foreach (ProjectItem item in project.Items) {
				ProjectItemInformation projectItem = ConvertToProjectItemInformation (item);
				if (projectItem != null) {
					yield return projectItem;
				}
			}
		}

		ProjectItemInformation ConvertToProjectItemInformation (ProjectItem item)
		{
			var fileItem = item as ProjectFile;
			if ((fileItem != null) && string.IsNullOrEmpty (fileItem.DependsOn)) {
				return ConvertFileToProjectItemInformation (fileItem);
			}
			return null;
		}

		ProjectItemInformation ConvertFileToProjectItemInformation (ProjectFile fileItem)
		{
			if (IsInProjectRootFolder (fileItem)) {
				if (IsDirectory (fileItem)) {
					return CreateDirectoryProjectItemInformationIfDirectoryNotAlreadyIncluded (fileItem);
				}
				return fileItem.ToProjectItemInformation ();
			}
			return ConvertDirectoryToProjectItemInformation (fileItem);
		}

		bool IsInProjectRootFolder (ProjectFile item)
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
			return !string.IsNullOrEmpty (directoryName);
		}

		bool IsDirectory (ProjectFile fileItem)
		{
			return fileItem.FilePath.IsDirectory;
		}

		ProjectItemInformation CreateDirectoryProjectItemInformationIfDirectoryNotAlreadyIncluded (ProjectFile fileItem)
		{
			string directory = fileItem.FilePath;
			if (!IsDirectoryIncludedAlready (directory)) {
				AddIncludedDirectory (directory);
				return fileItem.ToProjectItemInformation ();
			}
			return null;
		}

		ProjectItemInformation ConvertDirectoryToProjectItemInformation (ProjectFile fileItem)
		{
			string relativePath = project.GetRelativePath (fileItem.FilePath);
			string subDirectoryName = GetFirstSubDirectoryName (relativePath);
			if (IsDirectoryInsideProject (subDirectoryName)) {
				string fullPath = project.BaseDirectory.Combine (subDirectoryName);
				return CreateDirectoryProjectItemIfDirectoryNotAlreadyIncluded (subDirectoryName, fullPath);
			}
			return null;
		}

		ProjectItemInformation CreateDirectoryProjectItemIfDirectoryNotAlreadyIncluded (string subDirectoryName, string fullPath)
		{
			if (!IsDirectoryIncludedAlready (subDirectoryName)) {
				AddIncludedDirectory (subDirectoryName);
				return CreateDirectoryProjectItemInformation (subDirectoryName, fullPath);
			}
			return null;
		}

		bool IsDirectoryInsideProject (string directoryName)
		{
			return !directoryName.StartsWith ("..", StringComparison.Ordinal);
		}

		bool IsDirectoryIncludedAlready (string directory)
		{
			return directoriesIncluded.ContainsKey (directory);
		}

		void AddIncludedDirectory (string directoryName)
		{
			directoriesIncluded.Add (directoryName, directoryName);
		}

		ProjectItemInformation CreateDirectoryProjectItemInformation (string directoryName, string fullPath)
		{
			var directoryItem = new ProjectFile (fullPath);
			return directoryItem.ToProjectItemInformation ();
		}

		string GetFirstSubDirectoryName (string include)
		{
			string[] directoryNames = include.Split ('/');
			return directoryNames [0];
		}
	}
}

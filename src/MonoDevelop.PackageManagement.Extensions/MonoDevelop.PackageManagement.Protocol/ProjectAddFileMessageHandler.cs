//
// ProjectAddFileMessageHandler.cs
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
using System.IO;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.Protocol
{
	class ProjectAddFileMessageHandler
	{
		readonly Project project;
		readonly ProjectAddFileParams message;

		public ProjectAddFileMessageHandler (Project project, ProjectAddFileParams message)
		{
			this.project = project;
			this.message = message;
		}

		public async Task<ProjectItemInformation> AddFileAsync ()
		{
			string dependentUpon = GetDependentUpon (message.FileName);
			ProjectFile item = CreateFileProjectItemWithDependentUsingFullPath (message.FileName, dependentUpon);

			await Runtime.RunInMainThread (async () => {
				project.AddFile (item);
				await project.SaveAsync (new ProgressMonitor ());
			});

			return item.ToProjectItemInformation ();
		}

		string GetDependentUpon (string path)
		{
			var dependentFile = new DependentFile (project);
			ProjectFile projectItem = dependentFile.GetParentFileProjectItem (path);
			if (projectItem != null) {
				string relativePath = GetRelativePath (projectItem.FilePath);
				return Path.GetFileName (relativePath);
			}
			return null;
		}

		ProjectFile CreateFileProjectItemWithDependentUsingFullPath(string path, string dependentUpon)
		{
			ProjectFile fileProjectItem = CreateFileProjectItemUsingFullPath (path);
			fileProjectItem.DependsOn = dependentUpon;
			return fileProjectItem;
		}

		ProjectFile CreateFileProjectItemUsingFullPath (string path)
		{
			string relativePath = GetRelativePath (path);
			return CreateFileProjectItemUsingPathRelativeToProject (relativePath, path);
		}

		string GetRelativePath (string path)
		{
			return FileService.AbsoluteToRelativePath (project.BaseDirectory, path);
		}

		ProjectFile CreateFileProjectItemUsingPathRelativeToProject (string include, string fullPath)
		{
			var fileItem = new ProjectFile (fullPath);
			fileItem.BuildAction = project.GetDefaultBuildAction (fullPath);
			if (IsLink (include)) {
				fileItem.Link = fullPath;
			}
			return fileItem;
		}

		bool IsLink (string include)
		{
			return include.StartsWith ("..", StringComparison.Ordinal);
		}
	}
}

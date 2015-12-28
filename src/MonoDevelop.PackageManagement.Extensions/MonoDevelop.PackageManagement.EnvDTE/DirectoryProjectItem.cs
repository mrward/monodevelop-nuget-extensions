// 
// DirectoryProjectItem.cs
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
using System.IO;
using System.Linq;

using MD = MonoDevelop.Projects;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class DirectoryProjectItem : ProjectItem
	{
		string relativePath;

		public DirectoryProjectItem (
			Project project,
			string relativePath)
			: this (project, CreateFileProjectItem (project, relativePath))
		{
			this.relativePath = relativePath;
		}

		static MD.ProjectFile CreateFileProjectItem (Project project, string relativePath)
		{
			return new MD.ProjectFile (project.DotNetProject.BaseDirectory.Combine (relativePath));
		}

		static string GetSubdirectoryName (string relativePath)
		{
			return relativePath.Split ('\\').First ();
		}

		public DirectoryProjectItem (Project project, MD.ProjectFile projectItem)
			: base (project, projectItem)
		{
		}

		internal override bool IsChildItem (MD.ProjectItem msbuildProjectItem)
		{
			var fileItem = msbuildProjectItem as MD.ProjectFile;
			if (fileItem == null)
				return false;

			string relativePath = GetPathRelativeToProject (fileItem.FilePath);
			string directory = Path.GetDirectoryName (relativePath);
			if (directory == relativePath) {
				return true;
			}
			return false;
		}

		public static DirectoryProjectItem CreateDirectoryProjectItemFromFullPath (Project project, string directory)
		{
			string relativePath = project.GetRelativePath (directory);
			return new DirectoryProjectItem (project, relativePath);
		}
	}
}

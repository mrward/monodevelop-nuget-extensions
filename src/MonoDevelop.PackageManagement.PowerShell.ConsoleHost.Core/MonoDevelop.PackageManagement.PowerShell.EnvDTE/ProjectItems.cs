﻿//
// ProjectItems.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using MonoDevelop.PackageManagement.PowerShell.Protocol;

namespace MonoDevelop.PackageManagement.PowerShell.EnvDTE
{
	public class ProjectItems : MarshalByRefObject, IEnumerable, global::EnvDTE.ProjectItems
	{
		object parent;

		public ProjectItems (Project project, object parent)
		{
			Project = project;
			this.parent = parent;
		}

		public ProjectItems ()
		{
		}

		protected Project Project { get; private set; }

		public virtual object Parent {
			get { return parent; }
		}

		public virtual void AddFromFileCopy (string filePath)
		{
			throw new NotImplementedException ();
			//string fileAdded = filePath;
			//if (IsFileInsideProjectFolder (filePath)) {
			//	ThrowExceptionIfFileDoesNotExist (filePath);
			//} else {
			//	fileAdded = CopyFileIntoProject (filePath);
			//}

			//AddFileProjectItemToProject (fileAdded);
			//Project.Save ();
		}

		///// <summary>
		///// The file will be copied inside the folder for the parent containing 
		///// these project items.
		///// </summary>
		//string GetIncludePathForFileCopy (string filePath)
		//{
		//	string fileNameWithoutAnyPath = Path.GetFileName (filePath);
		//	if (Parent is Project) {
		//		return fileNameWithoutAnyPath;
		//	}
		//	var item = Parent as ProjectItem;
		//	return item.GetIncludePath (fileNameWithoutAnyPath);
		//}

		//bool IsFileInsideProjectFolder (string filePath)
		//{
		//	return Project.IsFileFileInsideProjectFolder (filePath);
		//}

		void ThrowExceptionIfFileDoesNotExist (string filePath)
		{
			if (!File.Exists (filePath)) {
				throw new FileNotFoundException ("Cannot find file", filePath);
			}
		}

		//void ThrowExceptionIfFileExists (string filePath)
		//{
		//	if (File.Exists (filePath)) {
		//		throw new FileExistsException (filePath);
		//	}
		//}

		//string CopyFileIntoProject (string fileName)
		//{
		//	string projectItemInclude = GetIncludePathForFileCopy (fileName);
		//	string newFileName = GetFileNameInProjectFromProjectItemInclude (projectItemInclude);
		//	ThrowExceptionIfFileExists (newFileName);
		//	FileService.CopyFile (fileName, newFileName);
		//	return newFileName;
		//}

		//string GetFileNameInProjectFromProjectItemInclude (string projectItemInclude)
		//{
		//	return Path.Combine (Project.DotNetProject.BaseDirectory, projectItemInclude);
		//}

		public virtual IEnumerator GetEnumerator ()
		{
			return GetProjectItems ().GetEnumerator ();
		}

		protected virtual IEnumerable<global::EnvDTE.ProjectItem> GetProjectItems ()
		{
			var message = new ProjectItemInformationParams {
				ProjectFileName = Project.FileName
			};
			var list = JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<ProjectItemInformationList> (
				Methods.ProjectItemsName,
				message).WaitAndGetResult ();

			foreach (ProjectItemInformation info in list.Items) {
				yield return new ProjectItem (Project, info);
			}
		}

		internal virtual global::EnvDTE.ProjectItem Item (string name)
		{
			foreach (ProjectItem item in this) {
				if (item.IsMatchByName (name)) {
					return item;
				}
			}
			return null;
		}

		internal virtual global::EnvDTE.ProjectItem Item (int index)
		{
			return GetProjectItems ()
				.Skip (index - 1)
				.First () as ProjectItem;
		}

		public virtual global::EnvDTE.ProjectItem Item (object index)
		{
			if (index is int) {
				return Item ((int)index);
			}
			return Item (index as string);
		}

		public virtual global::EnvDTE.ProjectItem AddFromDirectory (string directory)
		{
			throw new NotImplementedException ();
			//ProjectItem directoryItem = Project.AddDirectoryProjectItemUsingFullPath (directory);
			//Project.Save ();
			//return directoryItem;
		}

		public virtual global::EnvDTE.ProjectItem AddFromFile (string fileName)
		{
			if (Project is CpsProject) {
				return null;
			}

			var message = new ProjectAddFileParams {
				ProjectFileName = Project.FileName,
				FileName = fileName
			};
			var info = JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<ProjectItemInformation> (
				Methods.ProjectAddFileName,
				message).WaitAndGetResult ();

			return new ProjectItem (Project, info);
		}

		public virtual int Count {
			get { return GetProjectItems ().Count (); }
		}

		public virtual string Kind {
			get { return global::EnvDTE.Constants.vsProjectItemKindPhysicalFolder; }
		}
	}
}

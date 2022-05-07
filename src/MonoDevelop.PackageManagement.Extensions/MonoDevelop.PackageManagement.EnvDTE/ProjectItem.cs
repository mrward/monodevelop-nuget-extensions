// 
// ProjectItem.cs
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

using MD = MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class ProjectItem : MonoDevelop.EnvDTE.ProjectItemBase, global::EnvDTE.ProjectItem
	{
		MD.ProjectFile projectItem;
		Project containingProject;

		public const string CopyToOutputDirectoryPropertyName = "CopyToOutputDirectory";
		public const string CustomToolPropertyName = "CustomTool";
		public const string FullPathPropertyName = "FullPath";
		public const string LocalPathPropertyName = "LocalPath";

		public ProjectItem (Project project, MD.ProjectFile projectItem)
		{
			this.projectItem = projectItem;
			this.containingProject = project;
			this.ProjectItems = CreateProjectItems (projectItem);
			CreateProperties ();
			Kind = GetKindFromFileProjectItemType ();
		}

		global::EnvDTE.ProjectItems CreateProjectItems (MD.ProjectFile projectItem)
		{
			if (projectItem.FilePath.IsDirectory) {
				return new DirectoryProjectItems (this);
			}
			return new FileProjectItems (this);
		}

//		internal static ProjectItem FindByEntity (IProject project, IEntity entity)
//		{
//			if (entity.Region.FileName != null) {
//				return FindByFileName (project, entity.Region.FileName);
//			}
//			return null;
//		}

		internal static ProjectItem FindByFileName (MD.DotNetProject project, string fileName)
		{
//			MD.ProjectFile item = project.FindFile (new FileName (fileName));
//			if (item != null) {
//				return new ProjectItem (new Project (project), item);
//			}
			return null;
		}

		string GetKindFromFileProjectItemType ()
		{
			if (IsDirectory) {
				return global::EnvDTE.Constants.vsProjectItemKindPhysicalFolder;
			}
			return global::EnvDTE.Constants.vsProjectItemKindPhysicalFile;
		}

		bool IsDirectory {
			get { return projectItem.FilePath.IsDirectory; }
		}

		public ProjectItem ()
		{
		}

		void CreateProperties ()
		{
			var propertyFactory = new ProjectItemPropertyFactory (this);
			Properties = new Properties (propertyFactory);
		}

		public virtual string Name {
			get { return Path.GetFileName (projectItem.FilePath.FileName); }
		}

		public virtual string Kind { get; set; }

		public global::EnvDTE.Project SubProject {
			get { return null; }
		}

		public virtual global::EnvDTE.Properties Properties { get; private set; }

		public virtual global::EnvDTE.Project ContainingProject {
			get { return this.containingProject; }
		}

		public virtual global::EnvDTE.ProjectItems ProjectItems { get; private set; }

		internal virtual object GetProperty (string name)
		{
			if (name == CopyToOutputDirectoryPropertyName) {
				return GetCopyToOutputDirectory ();
			} else if (name == CustomToolPropertyName) {
//				return projectItem.CustomTool;
			} else if ((name == FullPathPropertyName) || (name == LocalPathPropertyName)) {
				return projectItem.FilePath.ToString ();
			}
			return String.Empty;
		}

		UInt32 GetCopyToOutputDirectory ()
		{
			return (UInt32)projectItem.CopyToOutputDirectory;
		}

		internal virtual void SetProperty (string name, object value)
		{
			if (name == CopyToOutputDirectoryPropertyName) {
				SetCopyToOutputDirectory (value);
			} else if (name == CustomToolPropertyName) {
//				projectItem.CustomTool = value as string;
			}
		}

		void SetCopyToOutputDirectory (object value)
		{
			MD.FileCopyMode copyToOutputDirectory = ConvertToCopyToOutputDirectory (value);
			projectItem.CopyToOutputDirectory = copyToOutputDirectory;
		}

		MD.FileCopyMode ConvertToCopyToOutputDirectory (object value)
		{
			string valueAsString = value.ToString ();
			return (MD.FileCopyMode)Enum.Parse (typeof(MD.FileCopyMode), valueAsString);
		}

		internal virtual bool IsMatchByName (string name)
		{
			return String.Equals (this.Name, name, StringComparison.InvariantCultureIgnoreCase);
		}

		internal virtual bool IsChildItem (MD.ProjectItem msbuildProjectItem)
		{
			var fileItem = msbuildProjectItem as MD.ProjectFile;
			if (fileItem != null) {
				string directory = fileItem.FilePath.ParentDirectory;
				string relativePath = GetPathRelativeToProject (directory);
				return IsMatchByName (relativePath);
			}
			return false;
		}

		internal virtual ProjectItemRelationship GetRelationship (MD.ProjectItem msbuildProjectItem)
		{
			return new ProjectItemRelationship (this, msbuildProjectItem);
		}

		public void Delete ()
		{
			Runtime.RunInMainThread (() => {
				containingProject.RemoveProjectItem (this);
				containingProject.DeleteFile (projectItem.FilePath);
				containingProject.Save ();
			}).Wait ();
		}

//		public global::EnvDTE.FileCodeModel2 FileCodeModel {
//			get {
//				if (!IsDirectory) {
//					return new FileCodeModel2 (CreateModelContext (), containingProject);
//				}
//				return null;
//			}
//		}

//		CodeModelContext CreateModelContext ()
//		{
//			return new CodeModelContext {
//				CurrentProject = containingProject.MSBuildProject,
//				FilteredFileName = projectItem.FileName
//			};
//		}

		internal string GetIncludePath (string fileName)
		{
			string relativeDirectory = GetProjectItemRelativePathToProject ();
			return Path.Combine (relativeDirectory, fileName);
		}

		string GetProjectItemRelativePathToProject ()
		{
			return containingProject.GetRelativePath (projectItem.FilePath);
		}

		internal string GetIncludePath ()
		{
			return projectItem.FilePath;
		}

		public virtual void Remove ()
		{
			Runtime.RunInMainThread (() => {
				containingProject.RemoveProjectItem (this);
				containingProject.Save ();
			}).Wait ();
		}

		internal MD.ProjectFile MSBuildProjectItem {
			get { return projectItem; }
		}

		protected override string GetFileNames (short index)
		{
			return FileName;
		}

		string FileName {
			get { return projectItem.FilePath; }
		}

		public virtual global::EnvDTE.Document Document {
			get { return GetOpenDocument (); }
		}

		Document GetOpenDocument ()
		{
			MonoDevelop.Ide.Gui.Document document = containingProject.GetOpenFile (FileName);
			if (document != null) {
				return new Document (FileName, document);
			}
			return null;
		}

		public virtual global::EnvDTE.Window Open (string viewKind)
		{
			containingProject.OpenFile (FileName);
			return null;
		}

		public virtual short FileCount {
			get { return 1; }
		}

		public global::EnvDTE.ProjectItems Collection {
			get {
				string relativePath = GetProjectItemRelativeDirectoryToProject ();
				if (String.IsNullOrEmpty (relativePath)) {
					return containingProject.ProjectItems;
				}
				var directoryProjectItem = new DirectoryProjectItem (containingProject, relativePath);
				return directoryProjectItem.ProjectItems;
			}
		}

		string GetProjectItemRelativeDirectoryToProject ()
		{
			return Path.GetDirectoryName (GetProjectItemRelativePathToProject ());
		}

		public void Save (string fileName = null)
		{
			Runtime.RunInMainThread (() => {
				MonoDevelop.Ide.Gui.Document document = containingProject.GetOpenFile (FileName);
				if (document != null) {
					document.Save ();
				}
			}).Wait ();
		}

		protected string GetPathRelativeToProject (string directory)
		{
			return containingProject.GetRelativePath (directory);
		}

		public bool SaveAs (string NewFileName)
		{
			throw new NotImplementedException ();
		}

		public void ExpandView ()
		{
		}

		public bool IsDirty { get; set; }

		string global::EnvDTE.ProjectItem.Name { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.DTE DTE => throw new NotImplementedException ();

		protected override bool GetIsOpen(string viewKind)
		{
			return false;
		}

		public object Object => throw new NotImplementedException ();

		protected override object GetExtender (string extender)
		{
			return null;
		}

		public object ExtenderNames => throw new NotImplementedException ();

		public string ExtenderCATID => throw new NotImplementedException ();

		public bool Saved { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.ConfigurationManager ConfigurationManager => throw new NotImplementedException ();

		global::EnvDTE.FileCodeModel global::EnvDTE.ProjectItem.FileCodeModel => throw new NotImplementedException ();
	}
}

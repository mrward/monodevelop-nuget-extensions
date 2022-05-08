// 
// Project.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2014 Matthew Ward
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
using MonoDevelop.Core;
using MonoDevelop.Projects;
using MD = MonoDevelop.Projects;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	public class Project : MonoDevelop.EnvDTE.ProjectBase, global::EnvDTE.Project
	{
		IExtendedPackageManagementProjectService projectService;
		IPackageManagementFileService fileService;
		DTE dte;

		public Project (DotNetProject project)
			: this (
				project,
				PackageManagementExtendedServices.ProjectService,
				new PackageManagementFileService ())
		{
		}

		internal Project (
			DotNetProject project,
			IExtendedPackageManagementProjectService projectService,
			IPackageManagementFileService fileService)
		{
			this.DotNetProject = project;
			this.projectService = projectService;
			this.fileService = fileService;

			CreateProperties ();
			Object = new ProjectObject (this);
			ProjectItems = new ProjectItems (this, this, fileService);
		}

		public Project ()
		{
		}

		internal IPackageManagementFileService FileService {
			get { return fileService; }
		}

		void CreateProperties ()
		{
			var propertyFactory = new ProjectPropertyFactory (this);
			Properties = new Properties (propertyFactory);
		}

		public virtual string Name {
			get { return DotNetProject.Name; }
		}

		public virtual string UniqueName {
			get { return DotNetProject.GetUniqueName (); }
		}

		public virtual string FileName {
			get { return DotNetProject.FileName; }
		}

		public virtual string FullName {
			get { return FileName; }
		}

		public virtual object Object { get; private set; }

		public virtual global::EnvDTE.Properties Properties { get; private set; }

		public virtual global::EnvDTE.ProjectItems ProjectItems { get; private set; }

		public virtual global::EnvDTE.DTE DTE {
			get {
				if (dte == null) {
					dte = new DTE (projectService, fileService);
				}
				return dte;
			}
		}

		public virtual string Type {
			get { return GetProjectType (); }
		}

		string GetProjectType ()
		{
			return ProjectType.GetProjectType (DotNetProject);
		}

		public virtual string Kind {
			get { return GetProjectKind (); }
		}

		string GetProjectKind ()
		{
			return new ProjectKind (this).Kind;
		}

		internal DotNetProject DotNetProject { get; private set; }

		public virtual void Save ()
		{
			Runtime.RunInMainThread (async () => {
				await DotNetProject.SaveAsync (new ProgressMonitor ());
			}).Wait ();
		}

		internal virtual void AddReference (string path)
		{
			Runtime.RunInMainThread (() => {
				if (!HasReference (path)) {
					if (Path.IsPathRooted (path)) {
						DotNetProject.AddReference (path);
					} else {
						var reference = MD.ProjectReference.CreateAssemblyFileReference (path);
						DotNetProject.References.Add (reference);
					}
				}
			}).Wait ();
		}

		bool HasReference (string include)
		{
			foreach (MD.ProjectReference reference in GetReferences ()) {
				if (IsReferenceMatch (reference, include)) {
					return true;
				}
			}
			return false;
		}

		void AddProjectItemToMSBuildProject (MD.ProjectFile projectItem)
		{
			DotNetProject.AddFile (projectItem);
		}

		internal IEnumerable<MD.ProjectReference> GetReferences ()
		{
			return DotNetProject
				.References
				.Where (reference => IsAssemblyReference (reference));
		}

		bool IsAssemblyReference (ProjectReference reference)
		{
			return reference.ReferenceType == MD.ReferenceType.Assembly ||
				reference.ReferenceType == MD.ReferenceType.Package;
		}

		bool IsReferenceMatch (MD.ProjectReference reference, string include)
		{
			return String.Equals (reference.Reference, include, StringComparison.InvariantCultureIgnoreCase);
		}

		internal void RemoveReference (MD.ProjectReference referenceItem)
		{
			DotNetProject.References.Remove (referenceItem);
		}

		internal ProjectItem AddFileProjectItemUsingFullPath (string path)
		{
			string dependentUpon = GetDependentUpon (path);
			return AddFileProjectItemWithDependentUsingFullPath (path, dependentUpon);
		}

		string GetDependentUpon (string path)
		{
			var dependentFile = new DependentFile (DotNetProject);
			MD.ProjectFile projectItem = dependentFile.GetParentFileProjectItem (path);
			if (projectItem != null) {
				string relativePath = GetRelativePath (projectItem.FilePath);
				return Path.GetFileName (relativePath);
			}
			return null;
		}

		internal ProjectItem AddFileProjectItemWithDependentUsingFullPath(string path, string dependentUpon)
		{
			MD.ProjectFile fileProjectItem = CreateFileProjectItemUsingFullPath (path);
			fileProjectItem.DependsOn = dependentUpon;
			AddProjectItemToMSBuildProject (fileProjectItem);
			return new ProjectItem (this, fileProjectItem);
		}

		MD.ProjectFile CreateFileProjectItemUsingPathRelativeToProject (string include, string fullPath)
		{
			var fileItem = new MD.ProjectFile (fullPath);
			if (IsLink (include)) {
				fileItem.Link = fullPath;
			}
			return fileItem;
		}

		bool IsLink (string include)
		{
			return include.StartsWith ("..");
		}

		MD.ProjectFile CreateFileProjectItemUsingFullPath (string path)
		{
			string relativePath = GetRelativePath (path);
			return CreateFileProjectItemUsingPathRelativeToProject (relativePath, path);
		}

		internal IList<string> GetAllPropertyNames ()
		{
			var names = new List<string> ();
//			foreach (ProjectPropertyElement propertyElement in DotNetProject.MSBuildProjectFile.Properties) {
//				names.Add(propertyElement.Name);
//			}
			names.Add ("TargetFrameworkMoniker");
			names.Add ("OutputFileName");
			names.Add ("FullPath");
			names.Add ("LocalPath");
			names.Add ("DefaultNamespace");
			return names;
		}

		public virtual global::EnvDTE.CodeModel CodeModel {
//			get { return new CodeModel(projectService.GetProjectContent(DotNetProject) ); }
			get { throw new NotImplementedException (); }
		}

		public virtual global::EnvDTE.ConfigurationManager ConfigurationManager {
			get { return new ConfigurationManager (this); }
			set { throw new NotImplementedException (); }
		}

		//		internal virtual string GetLowercaseFileExtension()
		//		{
		//			return Path.GetExtension(FileName).ToLowerInvariant();
		//		}

		internal virtual void DeleteFile (string fileName)
		{
			Runtime.RunInMainThread (() => {
				FileService.RemoveFile (fileName);
			}).Wait ();
		}

//		internal ProjectItem AddDirectoryProjectItemUsingFullPath (string directory)
//		{
//			AddDirectoryProjectItemsRecursively (directory);
//			return DirectoryProjectItem.CreateDirectoryProjectItemFromFullPath (this, directory);
//		}

//		void AddDirectoryProjectItemsRecursively (string directory)
//		{
//			string[] files = Directory.GetFiles (directory);
//			string[] childDirectories = Directory.GetDirectories (directory);
//			if (files.Any ()) {
//				foreach (string file in files) {
//					AddFileProjectItemUsingFullPath (file);
//				}
//			} else if (!childDirectories.Any ()) {
//				AddDirectoryProjectItemToMSBuildProject (directory);
//			}
//
//			foreach (string childDirectory in childDirectories) {
//				AddDirectoryProjectItemsRecursively (childDirectory);
//			}
//		}

//		void AddDirectoryProjectItemToMSBuildProject (string directory)
//		{
//			ProjectFile projectItem = CreateMSBuildProjectItemForDirectory (directory);
//			AddProjectItemToMSBuildProject (projectItem);
//		}

		ProjectFile CreateMSBuildProjectItemForDirectory (string directory)
		{
			return new ProjectFile (directory);
		}

		internal string GetRelativePath (string path)
		{
			return MonoDevelop.Core.FileService.AbsoluteToRelativePath (DotNetProject.BaseDirectory, path);
		}

		internal void RemoveProjectItem (ProjectItem projectItem)
		{
			Runtime.RunInMainThread (() => {
				DotNetProject.Files.Remove (projectItem.MSBuildProjectItem);
			}).Wait ();
		}

		//		internal ProjectItem FindProjectItem(string fileName)
		//		{
		//			SD.FileProjectItem item = DotNetProject.FindFile(fileName);
		//			if (item != null) {
		//				return new ProjectItem(this, item);
		//			}
		//			return null;
		//		}

		internal MonoDevelop.Ide.Gui.Document GetOpenFile (string fileName)
		{
			MonoDevelop.Ide.Gui.Document document = null;

			Runtime.RunInMainThread (() => {
				document = IdeApp.Workbench.GetDocument (fileName);
			}).Wait ();

			return document;
		}

		internal void OpenFile (string fileName)
		{
			Runtime.RunInMainThread (() => {
				IdeApp.Workbench.OpenDocument (fileName, DotNetProject, true);
			}).Wait ();
		}

		internal bool IsFileFileInsideProjectFolder (string filePath)
		{
			string relativePath = MonoDevelop.Core.FileService.AbsoluteToRelativePath (DotNetProject.BaseDirectory, filePath);
			return !relativePath.StartsWith ("..");
		}

		public void SaveAs (string NewFileName)
		{
			throw new NotImplementedException ();
		}

		public void Save (string FileName = "")
		{
			throw new NotImplementedException ();
		}

		public void Delete ()
		{
			throw new NotImplementedException ();
		}

		string global::EnvDTE.Project.Name { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public bool IsDirty { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.Projects Collection => throw new NotImplementedException ();

		protected override object GetExtender(string extenderName)
		{
			return null;
		}

		public object ExtenderNames => throw new NotImplementedException ();

		public string ExtenderCATID => throw new NotImplementedException ();

		public bool Saved { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.Globals Globals => throw new NotImplementedException ();

		public global::EnvDTE.ProjectItem ParentProjectItem => throw new NotImplementedException ();
	}
}

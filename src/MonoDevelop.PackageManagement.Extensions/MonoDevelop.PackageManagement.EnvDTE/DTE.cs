// 
// DTE.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//using Microsoft.VisualStudio.Shell;
using MD = MonoDevelop.Projects;
using MonoDevelop.PackageManagement;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class DTE : MonoDevelop.EnvDTE.DTEBase, global::EnvDTE.DTE//, IServiceProvider
	{
		IExtendedPackageManagementProjectService projectService;
		IPackageManagementFileService fileService;
		Solution solution;
		
		public DTE ()
			: this (PackageManagementExtendedServices.ProjectService, new PackageManagementFileService ())
		{
		}

		internal DTE (
			IExtendedPackageManagementProjectService projectService,
			IPackageManagementFileService fileService)
		{
			this.projectService = projectService;
			this.fileService = fileService;

			ItemOperations = new ItemOperations ();
		}

		public string Version {
			get { return "10.0"; }
		}

		public global::EnvDTE.Solution Solution {
			get {
				if (IsSolutionOpen) {
					CreateSolution ();
					return solution;
				}
				return null;
			}
		}

		bool IsSolutionOpen {
			get { return projectService.OpenSolution != null; }
		}

		void CreateSolution ()
		{
			if (!IsOpenSolutionAlreadyCreated ()) {
				solution = new Solution (projectService);
			}
		}

		bool IsOpenSolutionAlreadyCreated ()
		{
			if (solution != null) {
				return solution.IsOpen;
			}
			return false;
		}
		
		public global::EnvDTE.ItemOperations ItemOperations { get; private set; }

		protected override global::EnvDTE.Properties GetProperties(string category, string page)
		{
			var properties = new DTEProperties ();
			return properties.GetProperties (category, page);
		}

		public object ActiveSolutionProjects {
			get {
				if (IsSolutionOpen) {
					return Solution.Projects.OfType<Project> ().ToArray ();
				}
				return new Project [0];
			}
		}

		public global::EnvDTE.SourceControl SourceControl {
			get { return null; }
		}

		public void Quit ()
		{
			throw new NotImplementedException ();
		}

		public object GetObject (string Name)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Window OpenFile (string ViewKind, string FileName)
		{
			throw new NotImplementedException ();
		}

		public void ExecuteCommand (string CommandName, string CommandArgs = "")
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.wizardResult LaunchWizard (string VSZFile, ref object[] ContextParams)
		{
			throw new NotImplementedException ();
		}

		public string SatelliteDllPath (string Path, string Name)
		{
			throw new NotImplementedException ();
		}

		public string Name => throw new NotImplementedException ();

		public string FileName => throw new NotImplementedException ();

		public object CommandBars => throw new NotImplementedException ();

		public global::EnvDTE.Windows Windows => throw new NotImplementedException ();

		public global::EnvDTE.Events Events => throw new NotImplementedException ();

		public global::EnvDTE.AddIns AddIns => throw new NotImplementedException ();

		public global::EnvDTE.Window MainWindow => throw new NotImplementedException ();

		public global::EnvDTE.Window ActiveWindow => throw new NotImplementedException ();

		public global::EnvDTE.vsDisplay DisplayMode { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.Commands Commands => throw new NotImplementedException ();

		public global::EnvDTE.SelectedItems SelectedItems => throw new NotImplementedException ();

		public string CommandLineArguments => throw new NotImplementedException ();

		protected override bool GetIsOpenFile(string viewKind, string fileName)
		{
			return false;
		}

		global::EnvDTE.DTE global::EnvDTE.DTE.DTE => throw new NotImplementedException ();

		public int LocaleID => throw new NotImplementedException ();

		public global::EnvDTE.WindowConfigurations WindowConfigurations => throw new NotImplementedException ();

		public global::EnvDTE.Documents Documents => throw new NotImplementedException ();

		public global::EnvDTE.Document ActiveDocument => throw new NotImplementedException ();

		public global::EnvDTE.Globals Globals => throw new NotImplementedException ();

		public global::EnvDTE.StatusBar StatusBar => throw new NotImplementedException ();

		public string FullName => throw new NotImplementedException ();

		public bool UserControl { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.ObjectExtenders ObjectExtenders => throw new NotImplementedException ();

		public global::EnvDTE.Find Find => throw new NotImplementedException ();

		public global::EnvDTE.vsIDEMode Mode => throw new NotImplementedException ();

		public global::EnvDTE.UndoContext UndoContext => throw new NotImplementedException ();

		public global::EnvDTE.Macros Macros => throw new NotImplementedException ();

		public global::EnvDTE.DTE MacrosIDE => throw new NotImplementedException ();

		public string RegistryRoot => throw new NotImplementedException ();

		public global::EnvDTE.DTE Application => throw new NotImplementedException ();

		public global::EnvDTE.ContextAttributes ContextAttributes => throw new NotImplementedException ();

		public bool SuppressUI { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.Debugger Debugger => throw new NotImplementedException ();

		public string Edition => throw new NotImplementedException ();

		//		/// <summary>
		//		/// HACK - EnvDTE.DTE actually implements Microsoft.VisualStudio.OLE.Interop.IServiceProvider
		//		/// which is COM specific and has a QueryInterface method.
		//		/// </summary>
		//		object IServiceProvider.GetService(Type serviceType)
		//		{
		//			return Package.GetGlobalService(serviceType);
		//		}
	}
}

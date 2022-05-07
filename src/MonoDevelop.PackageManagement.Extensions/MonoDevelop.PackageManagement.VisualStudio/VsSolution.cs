//
// VsSolution.cs
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
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement.Scripting;
using NuGet.VisualStudio;

namespace MonoDevelop.PackageManagement.VisualStudio
{
	class VsSolution : MarshalByRefObject, IVsSolution, SVsSolution
	{
		public int GetProjectOfUniqueName (string uniqueName, out IVsHierarchy hierarchy)
		{
			hierarchy = null;
			global::EnvDTE.Project project = FindProject (uniqueName);
			if (project != null) {
				hierarchy = new FlavoredProject (project);
				return VsConstants.S_OK;
			}
			return VsConstants.E_FAIL;
		}

		global::EnvDTE.Project FindProject (string uniqueName)
		{
			return Runtime.JoinableTaskFactory.Run(async () =>
			{
				IConsoleHostSolutionManager solutionManager = ServiceLocator.GetInstance<IConsoleHostSolutionManager> ();
				var projects = await solutionManager.GetAllEnvDTEProjectsAsync();
				return projects
					.SingleOrDefault (project => ProjectUniqueNameMatches (project, uniqueName));
			});
		}

		bool ProjectUniqueNameMatches (global::EnvDTE.Project project, string uniqueName)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (project.UniqueName, uniqueName);
		}

		public int GetProjectEnum (uint grfEnumFlags, ref Guid rguidEnumOnlyThisType, out IEnumHierarchies ppenum)
		{
			throw new NotImplementedException ();
		}

		public int CreateProject (ref Guid rguidProjectType, string lpszMoniker, string lpszLocation, string lpszName, uint grfCreateFlags, ref Guid iidProject, out IntPtr ppProject)
		{
			throw new NotImplementedException ();
		}

		public int GenerateUniqueProjectName (string lpszRoot, out string pbstrProjectName)
		{
			throw new NotImplementedException ();
		}

		public int GetProjectOfGuid (ref Guid rguidProjectID, out IVsHierarchy ppHierarchy)
		{
			throw new NotImplementedException ();
		}

		public int GetGuidOfProject (IVsHierarchy pHierarchy, out Guid pguidProjectID)
		{
			throw new NotImplementedException ();
		}

		public int GetSolutionInfo (out string pbstrSolutionDirectory, out string pbstrSolutionFile, out string pbstrUserOptsFile)
		{
			throw new NotImplementedException ();
		}

		public int AdviseSolutionEvents (IVsSolutionEvents pSink, out uint pdwCookie)
		{
			throw new NotImplementedException ();
		}

		public int UnadviseSolutionEvents (uint dwCookie)
		{
			throw new NotImplementedException ();
		}

		public int SaveSolutionElement (uint grfSaveOpts, IVsHierarchy pHier, uint docCookie)
		{
			throw new NotImplementedException ();
		}

		public int CloseSolutionElement (uint grfCloseOpts, IVsHierarchy pHier, uint docCookie)
		{
			throw new NotImplementedException ();
		}

		public int GetProjectOfProjref (string pszProjref, out IVsHierarchy ppHierarchy, out string pbstrUpdatedProjref, VSUPDATEPROJREFREASON[] puprUpdateReason)
		{
			throw new NotImplementedException ();
		}

		public int GetProjrefOfProject (IVsHierarchy pHierarchy, out string pbstrProjref)
		{
			throw new NotImplementedException ();
		}

		public int GetProjectInfoOfProjref (string pszProjref, int propid, out object pvar)
		{
			throw new NotImplementedException ();
		}

		public int AddVirtualProject (IVsHierarchy pHierarchy, uint grfAddVPFlags)
		{
			throw new NotImplementedException ();
		}

		public int GetItemOfProjref (string pszProjref, out IVsHierarchy ppHierarchy, out uint pitemid, out string pbstrUpdatedProjref, VSUPDATEPROJREFREASON[] puprUpdateReason)
		{
			throw new NotImplementedException ();
		}

		public int GetProjrefOfItem (IVsHierarchy pHierarchy, uint itemid, out string pbstrProjref)
		{
			throw new NotImplementedException ();
		}

		public int GetItemInfoOfProjref (string pszProjref, int propid, out object pvar)
		{
			throw new NotImplementedException ();
		}

		public int GetProperty (int propid, out object pvar)
		{
			throw new NotImplementedException ();
		}

		public int SetProperty (int propid, object var)
		{
			throw new NotImplementedException ();
		}

		public int OpenSolutionFile (uint grfOpenOpts, string pszFilename)
		{
			throw new NotImplementedException ();
		}

		public int QueryEditSolutionFile (out uint pdwEditResult)
		{
			throw new NotImplementedException ();
		}

		public int CreateSolution (string lpszLocation, string lpszName, uint grfCreateFlags)
		{
			throw new NotImplementedException ();
		}

		public int GetProjectFactory (uint dwReserved, Guid[] pguidProjectType, string pszMkProject, out IVsProjectFactory ppProjectFactory)
		{
			throw new NotImplementedException ();
		}

		public int GetProjectTypeGuid (uint dwReserved, string pszMkProject, out Guid pguidProjectType)
		{
			throw new NotImplementedException ();
		}

		public int OpenSolutionViaDlg (string pszStartDirectory, int fDefaultToAllProjectsFilter)
		{
			throw new NotImplementedException ();
		}

		public int AddVirtualProjectEx (IVsHierarchy pHierarchy, uint grfAddVPFlags, ref Guid rguidProjectID)
		{
			throw new NotImplementedException ();
		}

		public int QueryRenameProject (IVsProject pProject, string pszMkOldName, string pszMkNewName, uint dwReserved, out int pfRenameCanContinue)
		{
			throw new NotImplementedException ();
		}

		public int OnAfterRenameProject (IVsProject pProject, string pszMkOldName, string pszMkNewName, uint dwReserved)
		{
			throw new NotImplementedException ();
		}

		public int RemoveVirtualProject (IVsHierarchy pHierarchy, uint grfRemoveVPFlags)
		{
			throw new NotImplementedException ();
		}

		public int CreateNewProjectViaDlg (string pszExpand, string pszSelect, uint dwReserved)
		{
			throw new NotImplementedException ();
		}

		public int GetVirtualProjectFlags (IVsHierarchy pHierarchy, out uint pgrfAddVPFlags)
		{
			throw new NotImplementedException ();
		}

		public int GenerateNextDefaultProjectName (string pszBaseName, string pszLocation, out string pbstrProjectName)
		{
			throw new NotImplementedException ();
		}

		public int GetProjectFilesInSolution (uint grfGetOpts, uint cProjects, string[] rgbstrProjectNames, out uint pcProjectsFetched)
		{
			throw new NotImplementedException ();
		}

		public int CanCreateNewProjectAtLocation (int fCreateNewSolution, string pszFullProjectFilePath, out int pfCanCreate)
		{
			throw new NotImplementedException ();
		}

		public int GetUniqueNameOfProject (IVsHierarchy pHierarchy, out string pbstrUniqueName)
		{
			throw new NotImplementedException ();
		}
	}
}

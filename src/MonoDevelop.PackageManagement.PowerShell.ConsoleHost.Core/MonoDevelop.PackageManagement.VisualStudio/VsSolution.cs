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
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;

namespace MonoDevelop.PackageManagement.VisualStudio
{
	public class VsSolution : MarshalByRefObject, IVsSolution, SVsSolution
	{
		public int GetProjectOfUniqueName (string uniqueName, out IVsHierarchy hierarchy)
		{
			hierarchy = null;
			EnvDTE.Project project = FindProject (uniqueName);
			if (project != null) {
				hierarchy = new FlavoredProject (project);
				return VsConstants.S_OK;
			}
			return VsConstants.E_FAIL;
		}

		EnvDTE.Project FindProject (string uniqueName)
		{
			return null;
			//var projects = ConsoleHostServices.SolutionManager.GetAllProjectsAsync ().WaitAndGetResult ();
			//return projects
			//	.SingleOrDefault (project => ProjectUniqueNameMatches (project, uniqueName));
		}

		bool ProjectUniqueNameMatches (EnvDTE.Project project, string uniqueName)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (project.UniqueName, uniqueName);
		}
	}
}

//
// FlavoredProject.cs
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
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Flavor
{
	public class FlavoredProject : MarshalByRefObject, IVsAggregatableProject, IVsHierarchy
	{
		EnvDTE.Project project;

		public FlavoredProject (EnvDTE.Project project)
		{
			this.project = project;
		}

		internal EnvDTE.Project Project {
			get { return project; }
		}

		public int GetAggregateProjectTypeGuids (out string projTypeGuids)
		{
			projTypeGuids = GetProjectTypeGuidsFromProject ();
			if (projTypeGuids == null) {
				projTypeGuids = project.Kind;
			}
			return VsConstants.S_OK;
		}

		string GetProjectTypeGuidsFromProject ()
		{
			// TODO: Implement
			return null;
			//return project.GetUnevalatedProperty ("ProjectTypeGuids");
		}

		public int SetInnerProject (object punkInner)
		{
			throw new NotImplementedException ();
		}

		public int InitializeForOuter (string pszFilename, string pszLocation, string pszName, uint grfCreateFlags, ref Guid iidProject, out IntPtr ppvProject, out int pfCanceled)
		{
			throw new NotImplementedException ();
		}

		public int OnAggregationComplete ()
		{
			throw new NotImplementedException ();
		}

		public int SetAggregateProjectTypeGuids (string lpstrProjTypeGuids)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.SetSite (OLE.Interop.IServiceProvider psp)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.GetSite (out OLE.Interop.IServiceProvider ppSP)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.QueryClose (out int pfCanClose)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.Close ()
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.GetGuidProperty (uint itemid, int propid, out Guid pguid)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.SetGuidProperty (uint itemid, int propid, ref Guid rguid)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.GetProperty (uint itemid, int propid, out object pvar)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.SetProperty (uint itemid, int propid, object var)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.GetNestedHierarchy (uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.GetCanonicalName (uint itemid, out string pbstrName)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.ParseCanonicalName (string pszName, out uint pitemid)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.Unused0 ()
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.AdviseHierarchyEvents (IVsHierarchyEvents pEventSink, out uint pdwCookie)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.UnadviseHierarchyEvents (uint dwCookie)
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.Unused1 ()
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.Unused2 ()
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.Unused3 ()
		{
			throw new NotImplementedException ();
		}

		int IVsHierarchy.Unused4 ()
		{
			throw new NotImplementedException ();
		}
	}
}

//
// Project.cs
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
using MonoDevelop.PackageManagement.PowerShell.Protocol;

namespace MonoDevelop.PackageManagement.PowerShell.EnvDTE
{
	public class Project : MarshalByRefObject, global::EnvDTE.Project
	{
		DTE dte;

		public Project (ProjectInformation info)
		{
			Name = info.Name;
			FileName = info.FileName;
		}

		public string Name { get; set; }

		public string UniqueName { get; set; }

		public string FileName { get; set; }

		public string FullName { get; set; }

		public object Object { get; set; }

		public global::EnvDTE.Properties Properties { get; set; }

		public global::EnvDTE.ProjectItems ProjectItems { get; set; }

		public global::EnvDTE.DTE DTE {
			get {
				if (dte == null) {
					dte = new DTE ();
				}
				return dte;
			}
		}

		public string Type { get; set; }

		public string Kind { get; set; }

		public global::EnvDTE.CodeModel CodeModel { get; set; }

		public global::EnvDTE.ConfigurationManager ConfigurationManager { get; set; }

		public void Save ()
		{
		}
	}
}

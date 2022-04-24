//
// Solution.cs
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
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;

namespace MonoDevelop.PackageManagement.PowerShell.EnvDTE
{
	public class Solution : MarshalByRefObject, global::EnvDTE.Solution
	{
		public Solution ()
		{
			//FileName = ConsoleHostServices.SolutionManager.SolutionFileName;
			Projects = new Projects (this);
			SolutionBuild = new SolutionBuild (this);
			var factory = new SolutionPropertyFactory (this);
			Properties = new Properties (factory);
		}

		public string FullName { get; private set; }

		public string FileName { get; private set; }

		public bool IsOpen { get; private set; }

		public global::EnvDTE.Projects Projects { get; private set; }

		public global::EnvDTE.Globals Globals { get; private set; }

		public global::EnvDTE.SolutionBuild SolutionBuild { get; private set; }

		public global::EnvDTE.Properties Properties { get; private set; }

		public global::EnvDTE.ProjectItem FindProjectItem (string fileName)
		{
			return null;
		}
	}
}

// 
// PowerShellWorkingDirectory.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
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
using MonoDevelop.Core;
using MonoDevelop.PackageManagement;
using MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.Scripting
{
	public class PowerShellWorkingDirectory
	{
		IExtendedPackageManagementProjectService projectService;
		
		public PowerShellWorkingDirectory(IExtendedPackageManagementProjectService projectService)
		{
			this.projectService = projectService;
		}
		
		public string GetWorkingDirectory()
		{
			Solution solution = projectService.OpenSolution;
			if (solution != null) {
				return QuotedDirectory(solution.BaseDirectory);
			}
			if (Platform.IsWindows) {
				return "$env:USERPROFILE";
			} else {
				return "$env:HOME";
			}
		}
		
		public static string QuotedDirectory (string directory)
		{
			return String.Format("'{0}'", directory);
		}
	}
}

// 
// ProjectKind.cs
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

using MD = MonoDevelop.Projects;
using MonoDevelop.PackageManagement;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class ProjectKind
	{
		const string CSharp = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
		const string VBNet = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";

		public ProjectKind(Project project)
		{
			Kind = GetProjectKind(project);
		}

		string GetProjectKind(Project project)
		{
			return GetProjectKind (project.DotNetProject);
		}

		internal static string GetProjectKind (MD.DotNetProject project)
		{
			string type = ProjectType.GetProjectType (project);
			if (type == ProjectType.CSharp) {
				return CSharp;
			} else if (type == ProjectType.VB) {
				return VBNet;
			}
			return string.Empty;
		}

		internal static string GetProjectKind (IDotNetProject project)
		{
			return GetProjectKind (project.DotNetProject);
		}

		public string Kind { get; private set; }
	}
}

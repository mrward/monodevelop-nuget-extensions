//
// ConsoleHostNuGetProjectServices.cs
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

using NuGet.ProjectManagement;
using MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.Scripting
{
	class ConsoleHostNuGetProjectServices : INuGetProjectServices
	{
		readonly INuGetProjectServices projectServices;

		public ConsoleHostNuGetProjectServices (
			DotNetProject project,
			NuGetProject nugetProject)
		{
			this.projectServices = nugetProject.ProjectServices;
			ScriptService = new ConsoleHostProjectScriptService (project);
		}

		public IProjectBuildProperties BuildProperties {
			get { return projectServices.BuildProperties; }
		}

		public IProjectSystemCapabilities Capabilities {
			get { return projectServices.Capabilities; }
		}

		public IProjectSystemReferencesReader ReferencesReader {
			get { return projectServices.ReferencesReader; }
		}

		public IProjectSystemReferencesService References {
			get { return projectServices.References; }
		}

		public IProjectSystemService ProjectSystem {
			get { return projectServices.ProjectSystem; }
		}

		public IProjectScriptHostService ScriptService { get; private set; }

		public T GetGlobalService<T> () where T : class
		{
			return projectServices.GetGlobalService<T>();
		}
	}
}

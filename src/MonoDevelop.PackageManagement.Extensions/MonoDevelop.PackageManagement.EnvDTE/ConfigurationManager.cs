//
// ConfigurationManager.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2022 Microsoft
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
using System.Collections;
using MonoDevelop.Ide;
using MD = MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	public class ConfigurationManager : MarshalByRefObject, global::EnvDTE.ConfigurationManager
	{
		Project project;

		public ConfigurationManager (Project project)
		{
			this.project = project;
		}

		public global::EnvDTE.Configuration ActiveConfiguration {
			get {
				return GetActiveConfiguration ();
			}
		}

		Configuration GetActiveConfiguration ()
		{
			var projectConfiguration = project.DotNetProject.GetConfiguration (IdeApp.Workspace.ActiveConfiguration) as MD.ProjectConfiguration;
			return new Configuration (project, projectConfiguration);
		}

		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Configuration Item (object index, string Platform = "")
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Configurations ConfigurationRow (string Name)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Configurations AddConfigurationRow (string NewName, string ExistingName, bool Propagate)
		{
			throw new NotImplementedException ();
		}

		public void DeleteConfigurationRow (string Name)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Configurations Platform (string Name)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Configurations AddPlatform (string NewName, string ExistingName, bool Propagate)
		{
			throw new NotImplementedException ();
		}

		public void DeletePlatform (string Name)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.DTE DTE => throw new NotImplementedException ();

		public object Parent => throw new NotImplementedException ();

		public int Count => throw new NotImplementedException ();

		public object ConfigurationRowNames => throw new NotImplementedException ();

		public object PlatformNames => throw new NotImplementedException ();

		public object SupportedPlatforms => throw new NotImplementedException ();
	}
}


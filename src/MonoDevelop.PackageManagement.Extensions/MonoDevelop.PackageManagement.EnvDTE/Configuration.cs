//
// Configuration.cs
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
using EnvDTE;
using MD = MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	public class Configuration : MonoDevelop.EnvDTE.ConfigurationBase, global::EnvDTE.Configuration
	{
		MD.ProjectConfiguration projectConfiguration;

		public Configuration (Project project, MD.ProjectConfiguration projectConfiguration)
		{
			Project = project;
			this.projectConfiguration = projectConfiguration;

			var propertyFactory = new ConfigurationPropertyFactory (this);
			Properties = new Properties (propertyFactory);
		}

		internal Project Project { get; }

		public global::EnvDTE.Properties Properties { get; private set; }

		public string ConfigurationName {
			get {
				var name = GetProperty ("ConfigurationName") as string;
				return name ?? string.Empty;
			}
		}

		internal object GetProperty (string name)
		{
			MD.MSBuild.IMetadataProperty property = projectConfiguration.Properties.GetProperty (name);
			if (property != null) {
				return property.GetEnvDTEValue ();
			}
			return string.Empty;
		}

		public global::EnvDTE.DTE DTE => throw new NotImplementedException ();

		public global::EnvDTE.ConfigurationManager Collection => throw new NotImplementedException ();

		public string PlatformName => throw new NotImplementedException ();

		public global::EnvDTE.vsConfigurationType Type => throw new NotImplementedException ();

		public object Owner => throw new NotImplementedException ();

		public bool IsBuildable => throw new NotImplementedException ();

		public bool IsRunable => throw new NotImplementedException ();

		public bool IsDeployable => throw new NotImplementedException ();

		public object Object => throw new NotImplementedException ();

		protected override object GetExtender (string extenderName)
		{
			return null;
		}

		public object ExtenderNames => throw new NotImplementedException ();

		public string ExtenderCATID => throw new NotImplementedException ();

		public OutputGroups OutputGroups => throw new NotImplementedException ();
	}
}


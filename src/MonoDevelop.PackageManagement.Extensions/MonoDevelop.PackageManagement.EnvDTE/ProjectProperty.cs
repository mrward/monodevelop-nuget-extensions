// 
// ProjectProperty.cs
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

using System;
using System.IO;
using MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	public class ProjectProperty : Property
	{
		Project project;

		public ProjectProperty (Project project, string name)
			: base (name)
		{
			this.project = project;
		}

		protected override object GetValue ()
		{
			if (IsTargetFrameworkMoniker ()) {
				return GetTargetFrameworkMoniker ();
			} else if (IsFullPath ()) {
				return GetFullPath ();
			} else if (IsOutputFileName ()) {
				return GetOutputFileName ();
			} else if (IsDefaultNamespace ()) {
				return GetDefaultNamespace ();
			} else if (IsIntermediateOutputPath ()) {
				return project.GetIntermediateOutputPath ();
			}

			string value = GetMSBuildProjectProperty (Name);
			if (value != null) {
				return value;
			}

			return EmptyStringIfNull (value);
		}

		string GetMSBuildProjectProperty (string name)
		{
			return project.DotNetProject.MSBuildProject.EvaluatedProperties.GetValue (name);
		}

		bool IsTargetFrameworkMoniker ()
		{
			return IsCaseInsensitiveMatch (Name, "TargetFrameworkMoniker");
		}

		bool IsFullPath ()
		{
			return IsCaseInsensitiveMatch (Name, "FullPath") || IsCaseInsensitiveMatch (Name, "LocalPath");
		}

		bool IsOutputFileName ()
		{
			return IsCaseInsensitiveMatch (Name, "OutputFileName");
		}

		bool IsIntermediateOutputPath ()
		{
			return IsCaseInsensitiveMatch (Name, "IntermediateOutputPath");
		}

		bool IsCaseInsensitiveMatch (string a, string b)
		{
			return string.Equals (a, b, StringComparison.OrdinalIgnoreCase);
		}

		string GetTargetFrameworkMoniker ()
		{
			var targetFramework = new ProjectTargetFramework (new DotNetProjectProxy (MSBuildProject));
			return targetFramework.TargetFrameworkName.ToString ();
		}

		bool IsDefaultNamespace ()
		{
			return IsCaseInsensitiveMatch (Name, "DefaultNamespace");
		}

		string GetDefaultNamespace ()
		{
			return MSBuildProject.DefaultNamespace;
		}

		DotNetProject MSBuildProject {
			get { return project.DotNetProject; }
		}

		string GetFullPath ()
		{
			return MSBuildProject.BaseDirectory + Path.DirectorySeparatorChar.ToString ();
		}

		string GetOutputFileName ()
		{
			return Path.GetFileName (MSBuildProject.GetOutputFileName (ConfigurationSelector.Default));
		}

		string EmptyStringIfNull (string value)
		{
			if (value != null) {
				return value;
			}
			return String.Empty;
		}

		protected override void SetValue (object value)
		{
//			bool escapeValue = false;
//			MSBuildProject.SetProperty (Name, value as string, escapeValue);
//			project.Save ();
		}
	}
}

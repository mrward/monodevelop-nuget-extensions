//
// ProjectProperty.cs
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
using System.IO;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using MonoDevelop.PackageManagement.PowerShell.Protocol;

namespace MonoDevelop.PackageManagement.PowerShell.EnvDTE
{
	class ProjectProperty : Property
	{
		Project project;
		string value = null;

		public ProjectProperty (Project project, string name)
			: base (name)
		{
			this.project = project;
		}

		protected override object GetValue ()
		{
			if (value != null) {
				return value;
			}

			if (IsFullPath ()) {
				return GetFullPath ();
			//} else if (IsOutputFileName ()) {
			//	return GetOutputFileName ();
			//} else if (IsDefaultNamespace ()) {
			//	return GetDefaultNamespace ();
			} else {
				value = GetMSBuildProjectProperty (Name);

			}
			value = EmptyStringIfNull (value);
			return value;
		}

		string GetMSBuildProjectProperty (string name)
		{
			var message = new ProjectPropertyParams {
				ProjectFileName = project.FileName,
				PropertyName = name
			};
			var result = JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<PropertyValueInfo> (Methods.ProjectPropertyValueName, message)
				.WaitAndGetResult ();
			return result.PropertyValue;
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

		bool IsCaseInsensitiveMatch (string a, string b)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (a, b);
		}

		bool IsDefaultNamespace ()
		{
			return IsCaseInsensitiveMatch (Name, "DefaultNamespace");
		}

		//string GetDefaultNamespace ()
		//{
		//	return MSBuildProject.RootNamespace;
		//}

		string GetFullPath ()
		{
			string directory = Path.GetDirectoryName (project.FileName);
			return directory + Path.DirectorySeparatorChar.ToString ();
		}

		//string GetOutputFileName ()
		//{
		//	return String.Format ("{0}{1}", MSBuildProject.AssemblyName, GetOutputTypeFileExtension ());
		//}

		//string GetOutputTypeFileExtension ()
		//{
		//	string outputTypeProperty = GetMSBuildProjectProperty ("OutputType");
		//	OutputType outputType = GetOutputType (outputTypeProperty);
		//	return CompilableProject.GetExtension (outputType);
		//}

		//OutputType GetOutputType (string outputTypeProperty)
		//{
		//	if (outputTypeProperty == null) {
		//		return OutputType.Exe;
		//	}

		//	OutputType outputType = OutputType.Exe;
		//	if (Enum.TryParse<OutputType> (outputTypeProperty, true, out outputType)) {
		//		return outputType;
		//	}
		//	return OutputType.Exe;
		//}

		static string EmptyStringIfNull (string value)
		{
			if (value != null) {
				return value;
			}
			return string.Empty;
		}

		protected override void SetValue (object value)
		{
			throw new NotImplementedException ("ProjectProperty.SetValue not implemented");
			//bool escapeValue = false;
			//MSBuildProject.SetProperty (Name, value as string, escapeValue);
			//project.Save ();
		}
	}
}

//
// ConfiguredProjectCommonProperties.cs
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
using System.Threading.Tasks;
using EnvDTE;
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;

namespace Microsoft.VisualStudio.Project
{
	public class ConfiguredProjectCommonProperties
	{
		readonly CpsProject project;

		internal ConfiguredProjectCommonProperties (CpsProject project)
		{
			this.project = project;
		}

		public Task<string> GetEvaluatedPropertyValueAsync (string propertyName)
		{
			if (IsIntermediateOutputPath (propertyName)) {
				return GetIntermediatePath ();
			}

			var property = project.Properties.Item (propertyName);
			return Task.FromResult (GetPropertyValue (property));
		}

		bool IsIntermediateOutputPath (string propertyName)
		{
			return StringComparer.OrdinalIgnoreCase.Equals ("IntermediateOutputPath", propertyName);
		}

		/// <summary>
		/// Special case 'IntermediateOutputPath'. For .NET Core projects this returns a path inside
		/// obj which uses guids and does not exist. Instead we ask the current configuration for the
		/// IntermediateOutputPath.
		/// </summary>
		Task<string> GetIntermediatePath ()
		{
			var property = project.ConfigurationManager.ActiveConfiguration.Properties.Item ("IntermediateOutputPath");
			return Task.FromResult (GetPropertyValue (property));
		}

		static string GetPropertyValue (EnvDTE.Property property)
		{
			if (property?.Value != null) {
				return property.Value.ToString ();
			}
			return string.Empty;
		}
	}
}

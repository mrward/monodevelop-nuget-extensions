//
// ProjectItemProperty.cs
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
using MonoDevelop.PackageManagement.PowerShell.Protocol;

namespace MonoDevelop.PackageManagement.PowerShell.EnvDTE
{
	class ProjectItemProperty : Property
	{
		public static readonly string CopyToOutputDirectoryPropertyName = "CopyToOutputDirectory";
		public static readonly string CustomToolPropertyName = "CustomTool";
		public static readonly string FullPathPropertyName = "FullPath";
		public static readonly string LocalPathPropertyName = "LocalPath";

		readonly ProjectItem projectItem;
		object value;

		public ProjectItemProperty (ProjectItem projectItem, string name)
			: base (name)
		{
			this.projectItem = projectItem;
		}

		protected override object GetValue ()
		{
			if (value != null) {
				return value;
			}

			if (StringComparer.OrdinalIgnoreCase.Equals (Name, FullPathPropertyName) ||
				StringComparer.OrdinalIgnoreCase.Equals (Name, LocalPathPropertyName)) {
				return projectItem.FileName;
			}

			value = GetMSBuildProjectProperty (Name);
			return value;
		}

		protected override void SetValue (object value)
		{
			SetProperty (value);
		}

		object GetMSBuildProjectProperty (string name)
		{
			var message = new ProjectItemPropertyParams {
				ProjectFileName = projectItem.ContainingProject.FileName,
				FileName = projectItem.FileName,
				PropertyName = name
			};
			var result = JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<PropertyValueInfo> (Methods.ProjectItemPropertyValueName, message)
				.WaitAndGetResult ();
			return result.PropertyValue;
		}

		void SetProperty (object propertyValue)
		{
			var message = new ProjectItemPropertyValueParams {
				ProjectFileName = projectItem.ContainingProject.FileName,
				FileName = projectItem.FileName,
				PropertyName = Name,
				PropertyValue = propertyValue
			};
			JsonRpcProvider.Rpc.InvokeAsync (Methods.ProjectItemSetPropertyValueName, message);
		}
	}
}

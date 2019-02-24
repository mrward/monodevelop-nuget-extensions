//
// ProjectItemExtensions.cs
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
using MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.Protocol
{
	static class ProjectItemExtensions
	{
		static readonly string CopyToOutputDirectory = nameof (CopyToOutputDirectory);
		static readonly string CustomTool = nameof (CustomTool);
		static readonly string ItemType = nameof (ItemType);

		public static object GetPropertyValue (this ProjectItem item, string propertyName)
		{
			var projectFile = item as ProjectFile;
			if (projectFile != null) {
				if (StringComparer.OrdinalIgnoreCase.Equals (ItemType, propertyName)) {
					return projectFile.BuildAction;
				} else if (StringComparer.OrdinalIgnoreCase.Equals (CustomTool, propertyName)) {
					return projectFile.Generator;
				} else if (StringComparer.OrdinalIgnoreCase.Equals (CopyToOutputDirectory, propertyName)) {
					return projectFile.GetCopyToOutputDirectory ();
				}
			}
			return item.Metadata.GetValue (propertyName);
		}

		static UInt32 GetCopyToOutputDirectory (this ProjectFile item)
		{
			return (UInt32)item.CopyToOutputDirectory;
		}

		public static void SetPropertyValue (this ProjectItem item, string propertyName, object propertyValue)
		{
			var projectFile = item as ProjectFile;
			if (projectFile != null) {
				if (StringComparer.OrdinalIgnoreCase.Equals (ItemType, propertyName)) {
					projectFile.BuildAction = propertyValue?.ToString ();
				} else if (StringComparer.OrdinalIgnoreCase.Equals (CustomTool, propertyName)) {
					projectFile.Generator = propertyValue?.ToString ();
				} else if (StringComparer.OrdinalIgnoreCase.Equals (CopyToOutputDirectory, propertyName)) {
					projectFile.SetCopyToOutputDirectory (propertyValue);
				}
			}
		}

		static void SetCopyToOutputDirectory (this ProjectFile item, object value)
		{
			FileCopyMode copyToOutputDirectory = ConvertToCopyToOutputDirectory (value);
			item.CopyToOutputDirectory = copyToOutputDirectory;
		}

		static FileCopyMode ConvertToCopyToOutputDirectory (object value)
		{
			string valueAsString = value.ToString ();
			return (FileCopyMode)Enum.Parse (typeof (FileCopyMode), valueAsString);
		}
	}
}

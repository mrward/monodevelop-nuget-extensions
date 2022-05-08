//
// IMetadataPropertyExtensions.cs
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
using System.IO;
using MonoDevelop.Projects.MSBuild;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	static class IMetadataPropertyExtensions
	{
		public static string GetEnvDTEValue (this IMetadataProperty property)
		{
			if (IsPathProperty (property.Name)) {
				return GetPathValue (property.Value);
			}

			return property.Value;
		}

		static string GetPathValue (string value)
		{
			if (value == null) {
				return value;
			}

			if (Path.DirectorySeparatorChar == '\\') {
				return value;
			}

			return value.Replace ('\\', '/');
		}

		static bool IsPathProperty (string name)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals ("OutputPath", name)) {
				return true;
			} else if (StringComparer.OrdinalIgnoreCase.Equals ("IntermediateOutputPath", name)) {
				return true;
			}
			return false;
		}
	}
}


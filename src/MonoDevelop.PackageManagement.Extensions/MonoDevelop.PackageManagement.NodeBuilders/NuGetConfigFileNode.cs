﻿//
// NuGetConfigFileNode.cs
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
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.PackageManagement.NodeBuilders
{
	class NuGetConfigFileNode
	{
		public NuGetConfigFileNode (
			FilePath baseDirectory,
			FilePath fileName,
			int index)
		{
			FileName = fileName;

			Name = GetDisplayName (baseDirectory, fileName);
			Index = index;
		}

		public FilePath FileName { get; }

		public string Name { get; private set; }

		public string IconId => Stock.XmlFileIcon;

		static string GetDisplayName (FilePath baseDirectory, FilePath fileName)
		{
			if (fileName.IsChildPathOf (baseDirectory)) {
				return FileService.AbsoluteToRelativePath (baseDirectory, fileName);
			}

			FilePath homeFolder = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			if (fileName.IsChildPathOf (homeFolder)) {
				string name = FileService.AbsoluteToRelativePath (homeFolder, fileName);
				return "~/" + name;
			}

			return fileName;
		}

		public int Index { get; private set; }
	}
}


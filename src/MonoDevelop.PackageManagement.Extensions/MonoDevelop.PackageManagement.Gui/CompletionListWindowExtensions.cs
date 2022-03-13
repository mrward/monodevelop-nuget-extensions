//
// CompletionListWindowExtensions.cs
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
/*
using System;
using System.Reflection;
using MonoDevelop.Ide.CodeCompletion;

namespace MonoDevelop.PackageManagement
{
	static class CompletionListWindowExtensions
	{
		/// <summary>
		/// Shows the completion window. The ShowListWindow method is internal
		/// so use reflection to call it. There are no public methods that can
		/// be used.
		/// </summary>
		public static bool ShowListWindow (
			this CompletionListWindow window,
			char firstChar,
			ICompletionDataList list,
			ICompletionWidget completionWidget,
			CodeCompletionContext completionContext)
		{
			MethodInfo[] methods = window.GetType ().GetMethods (BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (MethodInfo method in methods) {
				if (method.Name == "ShowListWindow") {
					if (method.GetParameters ().Length == 4) {
						return (bool)method.Invoke (window, new object[] { firstChar, list, completionWidget, completionContext });
					}
				}
			}
			throw new NotImplementedException ("CompletListWindow.ShowListWindow not found");
		}
	}
}
*/
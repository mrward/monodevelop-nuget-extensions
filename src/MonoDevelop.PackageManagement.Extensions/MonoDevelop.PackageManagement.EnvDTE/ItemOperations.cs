// 
// ItemOperations.cs
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
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class ItemOperations : MarshalByRefObject, global::EnvDTE.ItemOperations
	{
		public ItemOperations ()
		{
		}

		public void OpenFile (string fileName)
		{
			Runtime.RunInMainThread (() => {
				OpenFile (new FilePath (fileName));
			}).Wait ();
		}

		void OpenFile (FilePath filePath)
		{
			IdeApp.Workbench.OpenDocument (filePath, null, true);
		}

		public void Navigate (string url)
		{
			DesktopService.OpenFile (url);
		}
		
		public global::EnvDTE.Window NewFile (string fileName)
		{
			throw new NotImplementedException();
		}
	}
}

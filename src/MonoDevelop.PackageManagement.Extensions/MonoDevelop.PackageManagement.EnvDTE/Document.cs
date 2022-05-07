// 
// Document.cs
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
using EnvDTE;
using MonoDevelop.Core;
using MD = MonoDevelop.Ide.Gui;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class Document : MonoDevelop.EnvDTE.DocumentBase, global::EnvDTE.Document
	{
		MD.Document document;

		public Document (string fileName, MD.Document document)
		{
			this.FullName = fileName;
			this.document = document;
		}

		public virtual bool Saved {
			get { return !document.IsDirty; }
			set { 
				Runtime.RunInMainThread (() => {
					document.IsDirty = !value;
				});
			}
		}

		public string FullName { get; private set; }

		public Object Object (string modelKind)
		{
			throw new NotImplementedException();
		}

		public void Activate ()
		{
			throw new NotImplementedException ();
		}

		public void Close (vsSaveChanges Save = vsSaveChanges.vsSaveChangesPrompt)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Window NewWindow ()
		{
			throw new NotImplementedException ();
		}

		public bool Redo ()
		{
			throw new NotImplementedException ();
		}

		public bool Undo ()
		{
			throw new NotImplementedException ();
		}

		public vsSaveStatus Save (string FileName = "")
		{
			throw new NotImplementedException ();
		}

		public void PrintOut ()
		{
			throw new NotImplementedException ();
		}

		public void ClearBookmarks ()
		{
			throw new NotImplementedException ();
		}

		public bool MarkText (string Pattern, int Flags = 0)
		{
			throw new NotImplementedException ();
		}

		public bool ReplaceText (string FindText, string ReplaceText, int Flags = 0)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.DTE DTE => throw new NotImplementedException ();

		public string Kind => throw new NotImplementedException ();

		public Documents Collection => throw new NotImplementedException ();

		public global::EnvDTE.Window ActiveWindow => throw new NotImplementedException ();

		public string Name => throw new NotImplementedException ();

		public string Path => throw new NotImplementedException ();

		public bool ReadOnly { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public Windows Windows => throw new NotImplementedException ();

		public global::EnvDTE.ProjectItem ProjectItem => throw new NotImplementedException ();

		public object Selection => throw new NotImplementedException ();

		protected override object GetExtender (string extenderName)
		{
			return null;
		}

		public object ExtenderNames => throw new NotImplementedException ();

		public string ExtenderCATID => throw new NotImplementedException ();

		public int IndentSize => throw new NotImplementedException ();

		public string Language { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public int TabSize => throw new NotImplementedException ();

		public string Type => throw new NotImplementedException ();
	}
}

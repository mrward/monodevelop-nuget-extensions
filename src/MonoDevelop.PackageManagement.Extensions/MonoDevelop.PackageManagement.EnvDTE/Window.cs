// 
// Window.cs
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

namespace MonoDevelop.PackageManagement.EnvDTE
{
	public class Window : MonoDevelop.EnvDTE.WindowBase, global::EnvDTE.Window
	{
		public Window ()
		{
		}

		public global::EnvDTE.Document Document {
			get { throw new NotImplementedException(); }
		}

		public void SetFocus ()
		{
			throw new NotImplementedException ();
		}

		public void SetKind (global::EnvDTE.vsWindowType eKind)
		{
			throw new NotImplementedException ();
		}

		public void Detach ()
		{
			throw new NotImplementedException ();
		}

		public void Attach (IntPtr lWindowHandle)
		{
			throw new NotImplementedException ();
		}

		public void Activate ()
		{
			throw new NotImplementedException ();
		}

		public void Close (global::EnvDTE.vsSaveChanges SaveChanges = global::EnvDTE.vsSaveChanges.vsSaveChangesNo)
		{
			throw new NotImplementedException ();
		}

		public void SetSelectionContainer (ref object[] Objects)
		{
			throw new NotImplementedException ();
		}

		public void SetTabPicture (object Picture)
		{
			throw new NotImplementedException ();
		}

		public global::EnvDTE.Windows Collection => throw new NotImplementedException ();

		public bool Visible { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public int Left { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public int Top { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public int Width { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public int Height { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public global::EnvDTE.vsWindowState WindowState { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.vsWindowType Type => throw new NotImplementedException ();

		public global::EnvDTE.LinkedWindows LinkedWindows => throw new NotImplementedException ();

		public global::EnvDTE.Window LinkedWindowFrame => throw new NotImplementedException ();

		public IntPtr HWnd => throw new NotImplementedException ();

		public string Kind => throw new NotImplementedException ();

		public string ObjectKind => throw new NotImplementedException ();

		public object Object => throw new NotImplementedException ();

		protected override object GetDocumentData (string bstrWhichData)
		{
			return null;
		}

		public global::EnvDTE.ProjectItem ProjectItem => throw new NotImplementedException ();

		public global::EnvDTE.Project Project => throw new NotImplementedException ();

		public global::EnvDTE.DTE DTE => throw new NotImplementedException ();

		public object Selection => throw new NotImplementedException ();

		public bool Linkable { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public string Caption { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public bool IsFloating { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
		public bool AutoHides { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public global::EnvDTE.ContextAttributes ContextAttributes => throw new NotImplementedException ();
	}
}

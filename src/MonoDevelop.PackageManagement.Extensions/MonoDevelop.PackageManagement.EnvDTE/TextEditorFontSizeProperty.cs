// 
// TextEditorFontsSizeProperty.cs
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
using MonoDevelop.Ide;
using Pango;
using MonoDevelop.Core;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class TextEditorFontSizeProperty : Property
	{
		public TextEditorFontSizeProperty ()
			: base ("FontSize")
		{
		}

		protected override object GetValue ()
		{
			int fontSize = IdeServices.FontService.MonospaceFont.Size / 1024;
			return Convert.ToInt16 (fontSize);
		}

		protected override void SetValue (object value)
		{
			int fontSize = Convert.ToInt32 (value) * 1024;
			FontDescription fontDescription = IdeServices.FontService.MonospaceFont.Copy ();
			fontDescription.Size = fontSize;
			Runtime.RunInMainThread (() => {
				IdeServices.FontService.SetFont ("Editor", fontDescription.ToString ());
			}).Wait ();
		}
	}
}

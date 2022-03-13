//
// PackageConsoleCompletionWidget.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// Author: Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2013 Xamarin Inc. (www.xamarin.com)
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

// Based on code from DebuggerConsoleView
// https://github.com/mono/monodevelop/blob/master/main/src/addins/MonoDevelop.Debugger/MonoDevelop.Debugger/DebuggerConsoleView.cs
/*
using System;
using Gtk;
using MonoDevelop.Ide.CodeCompletion;

namespace MonoDevelop.PackageManagement
{
	class PackageConsoleCompletionWidget : ICompletionWidget
	{
		readonly PackageConsoleView view;

		public PackageConsoleCompletionWidget (PackageConsoleView view)
		{
			this.view = view;
			TextView = view.GetTextView ();
		}

		TextBuffer Buffer {
			get { return view.Buffer; }
		}

		TextIter Cursor {
			get { return view.Cursor; }
		}

		TextView TextView { get; set; }

		public CodeCompletionContext CurrentCodeCompletionContext {
			get {
				var widget = this as ICompletionWidget;
				return widget.CreateCodeCompletionContext (Position);
			}
		}

		EventHandler completionContextChanged;

		event EventHandler ICompletionWidget.CompletionContextChanged {
			add { completionContextChanged += value; }
			remove { completionContextChanged -= value; }
		}

		string TokenText {
			get { return Buffer.GetText (TokenBegin, TokenEnd, false); }
			set {
				TextIter start = TokenBegin;
				TextIter end = TokenEnd;

				Buffer.Delete (ref start, ref end);
				start = TokenBegin;
				Buffer.Insert (ref start, value);
			}
		}

		TextIter TokenBegin {
			get { return Buffer.GetIterAtMark (tokenBeginMark); }
		}

		TextIter TokenEnd {
			get { return Cursor; }
		}

		public TextMark tokenBeginMark;

		public int Position {
			get { return Cursor.Offset - TokenBegin.Offset; }
			set {
				throw new NotSupportedException ();
			}
		}

		public string GetText (int startOffset, int endOffset)
		{
			string text = TokenText;

			if (startOffset < 0 || startOffset > text.Length) {
				startOffset = 0;
			}

			if (endOffset > text.Length) {
				endOffset = text.Length;
			}

			return text.Substring (startOffset, endOffset - startOffset);
		}

		public void Replace (int offset, int count, string text)
		{
			if (count > 0) {
				TokenText = TokenText.Remove (offset, count);
			}

			if (!string.IsNullOrEmpty (text)) {
				TokenText = TokenText.Insert (offset, text);
			}
		}

		public int CaretOffset {
			get { return Position; }
			set { Position = value; }
		}

		public double ZoomLevel {
			get { return 1; }
		}

		public char GetChar (int offset)
		{
			string text = TokenText;

			if (offset >= text.Length) {
				return (char)0;
			}

			return text [offset];
		}

		public CodeCompletionContext CreateCodeCompletionContext (int triggerOffset)
		{
			int x, y;
			TextView.GdkWindow.GetOrigin (out x, out y);
			TextView.GetLineYrange (Cursor, out var lineY, out var height);

			Gdk.Rectangle rect = TextView.GetIterLocation (Cursor);

			y += lineY + height - (int)view.Vadjustment.Value;
			x += rect.X;

			return new CodeCompletionContext {
				TriggerXCoord = x,
				TriggerYCoord = y,
				TriggerTextHeight = height,
				TriggerOffset = triggerOffset,
				TriggerLineOffset = triggerOffset,
				TriggerWordLength = 0
			};
		}

		public string GetCompletionText (CodeCompletionContext ctx)
		{
			return TokenText.Substring (ctx.TriggerOffset, ctx.TriggerWordLength);
		}

		public void SetCompletionText (CodeCompletionContext ctx, string partial_word, string complete_word)
		{
			int cursorOffset = Position - (ctx.TriggerOffset + partial_word.Length);
			TextIter start = Buffer.GetIterAtOffset (TokenBegin.Offset + ctx.TriggerOffset);
			TextIter end = Buffer.GetIterAtOffset (start.Offset + partial_word.Length);
			Buffer.Delete (ref start, ref end);
			Buffer.Insert (ref start, complete_word);
			Buffer.PlaceCursor (Buffer.GetIterAtOffset (start.Offset + cursorOffset));
		}

		public void SetCompletionText (CodeCompletionContext ctx, string partial_word, string complete_word, int offset)
		{
			int cursorOffset = Position - (ctx.TriggerOffset + partial_word.Length);
			TextIter start = Buffer.GetIterAtOffset (TokenBegin.Offset + ctx.TriggerOffset);
			TextIter end = Buffer.GetIterAtOffset (start.Offset + partial_word.Length);
			Buffer.Delete (ref start, ref end);
			Buffer.Insert (ref start, complete_word);

			TextIter cursor = Buffer.GetIterAtOffset (start.Offset + offset + cursorOffset);
			Buffer.PlaceCursor (cursor);
		}

		public int TextLength {
			get { return TokenText.Length; }
		}

		public int SelectedLength {
			get { return 0; }
		}

		public Style GtkStyle {
			get { return view.Style; }
		}

		internal void OnUpdateInputLineBegin ()
		{
			if (tokenBeginMark == null) {
				tokenBeginMark = Buffer.CreateMark (null, Buffer.EndIter, true);
			} else {
				Buffer.MoveMark (tokenBeginMark, Buffer.EndIter);
			}
		}
	}
}
*/

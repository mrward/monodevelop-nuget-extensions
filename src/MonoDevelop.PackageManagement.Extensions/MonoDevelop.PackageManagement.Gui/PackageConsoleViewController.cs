﻿//
// PackageConsoleViewController.cs
//
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2014 Matthew Ward
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
using System.Threading.Tasks;
using AppKit;
using Microsoft.VisualStudio.Components;
using MonoDevelop.Components;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement.Scripting;
using NuGetConsole;

namespace MonoDevelop.PackageManagement
{
	class PackageConsoleViewController : IScriptingConsole
	{
		const int DefaultMaxVisibleColumns = 160;

		readonly ConsoleViewController controller;
		ICommandExpansion commandExpansion;

		int maxVisibleColumns = 0;

		public PackageConsoleViewController ()
		{
			controller = new ConsoleViewController (nameof (PackageConsoleViewController));

			controller.Editable = true;

			controller.ConsoleInput += OnConsoleInput;
			controller.TextView.IsKeyboardFocusedChanged += TextViewIsKeyboardFocusedChanged;
		}

		public NSView View {
			get { return controller.View; }
		}

		public void GrabFocus ()
		{
			NSView cocoaTextViewControl = controller.TextView.VisualElement;
			cocoaTextViewControl.Window?.MakeFirstResponder (cocoaTextViewControl);
		}

		public event EventHandler<ConsoleInputEventArgs> ConsoleInput;

		void OnConsoleInput (object sender, ConsoleInputEventArgs e)
		{
			ConsoleInput?.Invoke (sender, e);
		}

		public event EventHandler TextViewFocused;

		void TextViewIsKeyboardFocusedChanged (object sender, EventArgs e)
		{
			if (controller.TextView.IsKeyboardFocused) {
				TextViewFocused?.Invoke (sender, e);
			}
		}

		public void Dispose ()
		{
			controller.ConsoleInput -= OnConsoleInput;
			controller.TextView.IsKeyboardFocusedChanged -= TextViewIsKeyboardFocusedChanged;
		}

		void WriteOutputLine (string message, ScriptingStyle style)
		{
			WriteOutput (message + Environment.NewLine, GetLogLevel (style));
		}

		LogLevel GetLogLevel (ScriptingStyle style)
		{
			switch (style) {
				case ScriptingStyle.Error:
					return LogLevel.Error;
				case ScriptingStyle.Warning:
					return LogLevel.Warning;
				case ScriptingStyle.Debug:
					return LogLevel.Debug;
				default:
					return LogLevel.Default;
			}
		}

		public void WriteLine ()
		{
			Runtime.RunInMainThread (() => {
				controller.WriteOutput ("\n");
			}).Wait ();
		}

		public void WriteLine (string text, ScriptingStyle style)
		{
			Runtime.RunInMainThread (() => {
				if (style == ScriptingStyle.Prompt) {
					WriteOutputLine (text, style);
					ConfigurePromptString ();
					controller.Prompt (true);
				} else {
					WriteOutputLine (text, style);
				}
			}).Wait ();
		}

		void ConfigurePromptString ()
		{
			controller.PromptString = "PM> ";
		}

		public void Write (string text, ScriptingStyle style)
		{
			Runtime.RunInMainThread (() => {
				if (style == ScriptingStyle.Prompt) {
					ConfigurePromptString ();
					controller.Prompt (false);
				} else {
					controller.WriteOutput (text);
				}
			}).Wait ();
		}

		public void WriteOutput (string line, LogLevel logLevel)
		{
			Runtime.AssertMainThread ();

			//TextTag tag = GetTag (logLevel);
			//TextIter start = Buffer.EndIter;

			//if (tag == null) {
			//	Buffer.Insert (ref start, line);
			//} else {
			//	Buffer.InsertWithTags (ref start, line, tag);
			//}
			//Buffer.PlaceCursor (Buffer.EndIter);
			//TextView.ScrollMarkOnscreen (Buffer.InsertMark);

			controller.WriteOutput (line);
		}

		public bool ScrollToEndWhenTextWritten { get; set; }

		public void SendLine (string line)
		{
		}

		public void SendText (string text)
		{
		}

		public string ReadLine (int autoIndentSize)
		{
			throw new NotImplementedException ();
		}

		public string ReadFirstUnreadLine ()
		{
			throw new NotImplementedException ();
		}

		public int GetMaximumVisibleColumns ()
		{
			if (maxVisibleColumns > 0) {
				return maxVisibleColumns;
			}
			return DefaultMaxVisibleColumns;
		}

		void IScriptingConsole.Clear ()
		{
			Runtime.RunInMainThread (() => {
				controller.Clear ();
			});
		}

		public ICommandExpansion CommandExpansion {
			get { return commandExpansion; }
			set {
				commandExpansion = value;
				controller.TextView.Properties[typeof (ICommandExpansion)] = value;
			}
		}

		TaskCompletionSource<string> userInputTask;

		public Task<string> PromptForInput (string message)
		{
			return Runtime.RunInMainThread (() => {
				string originalPromptString = controller.PromptString;
				try {
					controller.PromptString = message;
					controller.Prompt (false);

					userInputTask = new TaskCompletionSource<string> ();
					return userInputTask.Task;
				} finally {
					controller.PromptString = originalPromptString;
				}
			});
		}

		public void StopWaitingForPromptInput ()
		{
			if (userInputTask != null) {
				userInputTask.TrySetCanceled ();
				userInputTask = null;
				controller.WriteOutput ("\n");
			}
		}
	}
}


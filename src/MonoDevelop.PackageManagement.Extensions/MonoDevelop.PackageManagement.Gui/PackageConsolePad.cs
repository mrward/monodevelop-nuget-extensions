// 
// PackageConsolePad.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2014 Matthew Ward
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
using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.Components;
using MonoDevelop.Components.Docking;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide;

namespace MonoDevelop.PackageManagement
{
	public class PackageConsolePad : PadContent
	{
		PackageConsoleView view;
		PackageManagementConsoleViewModel viewModel;
		PackageConsoleToolbarWidget toolbarWidget;
		
		public PackageConsolePad ()
		{
		}
		
		public override Control Control {
			get { return view; }
		}
		
		protected override void Initialize (IPadWindow window)
		{
			CreateToolbar (window);
			CreatePackageConsoleView ();
			CreatePackageConsoleViewModel ();
			BindingViewModelToView ();
		}
		
		void CreatePackageConsoleViewModel()
		{
			var consoleHostProvider = new PackageManagementConsoleHostProvider ();

			PackageManagementExtendedServices.ConsoleHost = consoleHostProvider.ConsoleHost;

			viewModel = new PackageManagementConsoleViewModel (
				PackageManagementServices.ProjectService,
				consoleHostProvider.ConsoleHost
			);
			viewModel.RegisterConsole (view);
		}

		void CreateToolbar (IPadWindow window)
		{
			toolbarWidget = new PackageConsoleToolbarWidget ();
			toolbarWidget.ClearButtonClicked += ClearButtonClicked;
			DockItemToolbar toolbar = window.GetToolbar (DockPositionType.Top);
			toolbar.Add (toolbarWidget, false);
			toolbar.ShowAll ();
		}

		void ClearButtonClicked(object sender, EventArgs e)
		{
			viewModel.ClearConsole ();
		}

		void CreatePackageConsoleView ()
		{
			view = new PackageConsoleView ();
			view.ConsoleInput += OnConsoleInput;
			view.TextViewFocused += TextViewFocused;
			view.ShadowType = Gtk.ShadowType.None;
			view.ShowAll ();
		}

		void OnConsoleInput (object sender, ConsoleInputEventArgs e)
		{
			viewModel.ProcessUserInput (e.Text);
		}
		
		public void RedrawContent ()
		{
		}
		
		public override void Dispose ()
		{
			view.ConsoleInput -= OnConsoleInput;
			view.TextViewFocused -= TextViewFocused;
		}
		
		void BindingViewModelToView ()
		{
			toolbarWidget.LoadViewModel (viewModel);
		}

		void TextViewFocused (object sender, EventArgs e)
		{
			viewModel.UpdatePackageSources ();
		}
	}
}

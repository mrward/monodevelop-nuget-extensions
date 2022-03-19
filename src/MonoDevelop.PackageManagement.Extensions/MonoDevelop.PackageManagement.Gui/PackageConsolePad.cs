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
using System.Collections.Specialized;
using MonoDevelop.Components;
using MonoDevelop.Components.Declarative;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.PackageManagement.Scripting;
using MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement
{
	class PackageConsolePad : PadContent
	{
		PackageConsoleViewController packageConsoleViewController;
		PackageManagementConsoleViewModel viewModel;
		ToolbarPopUpButtonItem packageSourcesComboBox;
		ToolbarButtonItem configurePackageSourceButton;
		ToolbarPopUpButtonItem projectsComboBox;
		ToolbarButtonItem clearButton;
		ToolbarButtonItem stopButton;
		bool reloadingPackageSources;
		bool reloadingProjects;

		public PackageConsolePad ()
		{
		}

		public override Control Control {
			get { return packageConsoleViewController.View; }
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
			viewModel.RegisterConsole (packageConsoleViewController);
		}

		void CreateToolbar (IPadWindow window)
		{
			var toolbar = new Toolbar ();

			var packageSourceLabel = new ToolbarLabelItem (
				toolbar.Properties,
				"packageSourceLabel",
				GettextCatalog.GetString ("Package Source:"));
			toolbar.AddItem (packageSourceLabel);

			packageSourcesComboBox = new ToolbarPopUpButtonItem (
				toolbar.Properties,
				nameof (packageSourcesComboBox));
			packageSourcesComboBox.Menu = new ContextMenu ();
			toolbar.AddItem (packageSourcesComboBox);

			configurePackageSourceButton = new ToolbarButtonItem (
				toolbar.Properties,
				nameof (configurePackageSourceButton));
			configurePackageSourceButton.Icon = "md-prefs-generic";
			configurePackageSourceButton.Clicked += ConfigurePackageSourceButtonClicked;
			configurePackageSourceButton.Tooltip = GettextCatalog.GetString ("Configure Package Sources");
			toolbar.AddItem (configurePackageSourceButton);

			var projectLabel = new ToolbarLabelItem (
				toolbar.Properties,
				"projectLabel",
				GettextCatalog.GetString ("Default Project:"));
			toolbar.AddItem (projectLabel);

			projectsComboBox = new ToolbarPopUpButtonItem (
				toolbar.Properties,
				nameof (projectsComboBox));
			projectsComboBox.Menu = new ContextMenu ();
			toolbar.AddItem (projectsComboBox);

			clearButton = new ToolbarButtonItem (
				toolbar.Properties,
				nameof (clearButton));
			clearButton.Icon = Stock.Broom;
			clearButton.Tooltip = GettextCatalog.GetString ("Clear Console");
			clearButton.Clicked += OnClearButtonClicked;
			toolbar.AddItem (clearButton);

			stopButton = new ToolbarButtonItem (
				toolbar.Properties,
				nameof (stopButton));
			stopButton.Icon = Stock.Stop;
			stopButton.Tooltip = GettextCatalog.GetString ("Stop command");
			stopButton.Enabled = false;
			stopButton.Clicked += OnStopButtonClicked;
			toolbar.AddItem (stopButton);

			window.SetToolbar (toolbar, DockPositionType.Top);

			// Workaround being unable to set the button's Enabled state before the underlying
			// NSView is created. It is created after SetToolbar is called.
			stopButton.Enabled = false;
		}

		public override void Dispose ()
		{
			configurePackageSourceButton.Clicked -= ConfigurePackageSourceButtonClicked;
			clearButton.Clicked -= OnClearButtonClicked;
			stopButton.Clicked -= OnStopButtonClicked;

			viewModel.PackageSources.CollectionChanged -= ViewModelPackageSourcesChanged;
			RemovePackageSourcesComboBoxEventHandlers ();

			viewModel.Projects.CollectionChanged -= ViewModelProjectsChanged;
			RemoveProjectsComboBoxEventHandlers ();

			viewModel.RunningCommand -= RunningCommand;
			viewModel.CommandCompleted -= CommandCompleted;

			packageConsoleViewController.ConsoleInput -= OnConsoleInput;
			//view.TextViewFocused -= TextViewFocused;
			//view.MaxVisibleColumnsChanged -= MaxVisibleColumnsChanged;
		}

		void CreatePackageConsoleView ()
		{
			packageConsoleViewController = new PackageConsoleViewController ();
			packageConsoleViewController.ConsoleInput += OnConsoleInput;
			//view.TextViewFocused += TextViewFocused;
			//view.MaxVisibleColumnsChanged += MaxVisibleColumnsChanged;
		}

		void OnConsoleInput (object sender, ConsoleInputEventArgs e)
		{
			viewModel.ProcessUserInput (e.Text);
		}

		public override void FocusPad ()
		{
			packageConsoleViewController?.GrabFocus ();
		}

		/*
		void TextViewFocused (object sender, EventArgs e)
		{
			viewModel.UpdatePackageSources ();
		}

		void MaxVisibleColumnsChanged (object sender, EventArgs e)
		{
			viewModel.OnMaxVisibleColumnsChanged ();
		}*/

		void ConfigurePackageSourceButtonClicked (object sender, EventArgs e)
		{
			IdeApp.Workbench.ShowGlobalPreferencesDialog (null, "PackageSources");
		}

		void OnClearButtonClicked (object sender, EventArgs e)
		{
			viewModel.ClearConsole ();
		}

		void OnStopButtonClicked (object sender, EventArgs e)
		{
			viewModel.StopCommand ();
		}

		void BindingViewModelToView ()
		{
			LoadPackageSources ();

			LoadProjects ();

			RegisterEvents ();
		}

		void RegisterEvents ()
		{
			viewModel.PackageSources.CollectionChanged += ViewModelPackageSourcesChanged;

			viewModel.Projects.CollectionChanged += ViewModelProjectsChanged;

			viewModel.RunningCommand += RunningCommand;
			viewModel.CommandCompleted += CommandCompleted;
		}

		void LoadPackageSources ()
		{
			RemovePackageSourcesComboBoxEventHandlers ();

			var menu = new ContextMenu ();

			foreach (SourceRepositoryViewModel packageSource in viewModel.PackageSources) {
				var menuItem = new ContextMenuItem ();
				menuItem.Label = packageSource.Name;
				menuItem.Context = packageSource;
				menuItem.Clicked += PackageSourcesComboBoxChanged;

				menu.Items.Add (menuItem);
			}

			packageSourcesComboBox.Menu = menu;

			packageSourcesComboBox.SetSelected (GetActivePackageSourceIndexFromViewModel ());
		}

		void RemovePackageSourcesComboBoxEventHandlers ()
		{
			foreach (ContextMenuItem menu in packageSourcesComboBox.Menu.Items) {
				menu.Clicked -= PackageSourcesComboBoxChanged;
			}
		}

		void RemoveProjectsComboBoxEventHandlers ()
		{
			foreach (ContextMenuItem menu in projectsComboBox.Menu.Items) {
				menu.Clicked -= ProjectsComboBoxChanged;
			}
		}

		void ViewModelPackageSourcesChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			reloadingPackageSources = true;

			LoadPackageSources ();

			reloadingPackageSources = false;
		}

		int GetActivePackageSourceIndexFromViewModel ()
		{
			if (viewModel.ActivePackageSource == null) {
				if (viewModel.PackageSources.Count > 0) {
					return 0;
				}
				return -1;
			}

			int index = viewModel.PackageSources.IndexOf (viewModel.ActivePackageSource);
			if (index >= 0) {
				return index;
			}

			return 0;
		}

		void PackageSourcesComboBoxChanged (object sender, ContextMenuItemClickedEventArgs e)
		{
			if (reloadingPackageSources)
				return;

			var packageSource = (SourceRepositoryViewModel)e.Context;

			viewModel.ActivePackageSource = packageSource;
		}

		void LoadProjects ()
		{
			RemoveProjectsComboBoxEventHandlers ();

			var menu = new ContextMenu ();

			foreach (Project project in viewModel.Projects) {
				var menuItem = new ContextMenuItem ();
				menuItem.Label = project.Name;
				menuItem.Context = project;
				menuItem.Clicked += ProjectsComboBoxChanged;

				menu.Items.Add (menuItem);
			}

			projectsComboBox.Menu = menu;

			if (viewModel.Projects.Count > 0) {
				projectsComboBox.SetSelected (0);
			}
		}

		void ViewModelProjectsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			reloadingProjects = true;

			LoadProjects ();

			reloadingProjects = false;
		}

		void ProjectsComboBoxChanged (object sender, ContextMenuItemClickedEventArgs e)
		{
			if (reloadingProjects)
				return;

			var project = (Project)e.Context;
			UpdateDefaultProject (project);
		}

		void UpdateDefaultProject (Project project)
		{
			viewModel.DefaultProject = project;
		}

		void RunningCommand (object sender, EventArgs e)
		{
			stopButton.Enabled = true;
		}

		void CommandCompleted (object sender, EventArgs e)
		{
			Runtime.RunInMainThread (() => {
				stopButton.Enabled = false;
			}).Ignore ();
		}
	}
}

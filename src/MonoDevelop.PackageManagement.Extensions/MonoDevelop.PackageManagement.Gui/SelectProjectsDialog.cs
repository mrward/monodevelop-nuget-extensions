//
// ManagePackagesDialog.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
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

using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Core;

namespace MonoDevelop.PackageManagement
{
	partial class SelectProjectsDialog
	{
		SelectProjectsViewModel viewModel;

		public SelectProjectsDialog (
			IEnumerable<IDotNetProject> projects,
			int packagesCount)
		{
			Build ();

			viewModel = new SelectProjectsViewModel (projects, packagesCount);

			UpdateTopLabel (projects.Count ());

			AddProjects ();
		}

		public IEnumerable<IDotNetProject> GetSelectedProjects ()
		{
			return viewModel.GetSelectedProjects ();
		}

		void UpdateTopLabel (int projectsCount)
		{
			if (viewModel.IsAddingSinglePackage) {
				topLabel.Text = GettextCatalog.GetPluralString (
					"Add the package to the project:",
					"Select the projects to add the package to:",
					projectsCount);
			} else if (viewModel.IsAddingMultiplePackages) {
				topLabel.Text = GettextCatalog.GetPluralString (
					"Add the packages to the project:",
					"Select the projects to add packages to:",
					projectsCount);
			}
		}

		void AddProjects ()
		{
			foreach (SelectedProjectViewModel project in viewModel.Projects) {
				AddProject (project);
			}

			UpdateOkButtonSensitivity ();
		}
	}
}

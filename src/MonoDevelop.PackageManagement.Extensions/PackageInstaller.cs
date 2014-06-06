//
// PackageInstaller.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
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
//

using System;
using ICSharpCode.PackageManagement;

namespace MonoDevelop.PackageManagement
{
	public class PackageInstaller
	{
		public void Run (InstallPackageCommand command)
		{
			try {
				IPackageManagementProject project = PackageManagementServices.Solution.GetActiveProject ();
				var action = new InstallPackageAction (project, PackageManagementServices.PackageManagementEvents);
				action.PackageId = command.PackageId;
				action.PackageVersion = command.GetVersion ();
				ProgressMonitorStatusMessage progressMessage = CreateProgressMessage (action.PackageId);
				PackageManagementServices.BackgroundPackageActionRunner.Run (progressMessage, action);
			} catch (Exception ex) {
				ShowStatusBarError (ex);
			}
		}

		ProgressMonitorStatusMessage CreateProgressMessage (string packageId)
		{
			return ProgressMonitorStatusMessageFactory.CreateInstallingSinglePackageMessage (packageId);
		}

		void ShowStatusBarError (Exception ex)
		{
			ProgressMonitorStatusMessage message = ProgressMonitorStatusMessageFactory.CreateInstallingSinglePackageMessage ("");
			PackageManagementServices.BackgroundPackageActionRunner.ShowError (message, ex);
		}
	}
}


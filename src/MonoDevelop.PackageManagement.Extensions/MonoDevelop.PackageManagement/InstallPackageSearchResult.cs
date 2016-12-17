//
// InstallPackageSearchResult.cs
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
//

using System;
using MonoDevelop.Core;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Ide;

namespace MonoDevelop.PackageManagement
{
	public class InstallPackageSearchResult : SearchResult
	{
		readonly InstallPackageCommand command;

		public InstallPackageSearchResult (InstallPackageCommand command)
			: base ("", "", 0)
		{
			this.command = command;
		}

		public override bool CanActivate {
			get { return IsProjectSelected () && command.IsValid; }
		}

		public override string GetMarkupText (bool selected)
		{
			return GettextCatalog.GetString (
				"Add Package <b>{0}</b>{1}",
				command.PackageId, GetPackageVersionMarkup ());
		}

		public override void Activate ()
		{
			var runner = new PackageInstaller ();
			runner.Run (command);
		}

		string GetPackageVersionMarkup ()
		{
			if (command.HasVersion ()) {
				return " <b>" + command.Version + "</b>";
			}
			return String.Empty;
		}

		bool IsProjectSelected ()
		{
			try {
				return IdeApp.ProjectOperations.CurrentSelectedProject != null;
			} catch (Exception ex) {
				LoggingService.LogError ("Error getting current project.", ex);
			}
			return false;
		}

		public bool CanBeDisplayed ()
		{
			return IsProjectSelected ();
		}
	}
}


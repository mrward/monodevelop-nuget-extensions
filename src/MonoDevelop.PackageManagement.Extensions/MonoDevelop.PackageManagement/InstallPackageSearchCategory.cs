//
// InstallPackageSearchCategory.cs
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
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Core;

namespace MonoDevelop.PackageManagement
{
	public class InstallPackageSearchCategory : SearchCategory
	{
		string[] tags = new [] {"command"};

		public InstallPackageSearchCategory ()
			: base (GettextCatalog.GetString("Command"))
		{
		}

		public override Task GetResults (
			ISearchResultCallback searchResultCallback,
			SearchPopupSearchPattern pattern,
			CancellationToken token)
		{
			if (pattern.Tag == null || IsValidTag (pattern.Tag)) {
				var command = new InstallPackageCommand (pattern.Pattern);
				var result = new InstallPackageSearchResult (command);
				if (result.CanBeDisplayed ()) {
					searchResultCallback.ReportResult (result);
				}
			}
			return Task.FromResult (0);
		}

		public override bool IsValidTag (string tag)
		{
			return tag == "command";
		}

		public override string[] Tags {
			get { return tags; }
		}
	}
}


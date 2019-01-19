//
// ConsoleHostPackageSourceProvider.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
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

using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Configuration;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core
{
	class ConsoleHostPackageSourceProvider : IPackageSourceProvider
	{
		List<PackageSource> packageSources = new List<PackageSource> ();

		public string ActivePackageSourceName { get; set; }
		public string DefaultPushSource { get; set; }

		public event EventHandler PackageSourcesChanged;

		public void DisablePackageSource (PackageSource source)
		{
		}

		public bool IsPackageSourceEnabled (PackageSource source)
		{
			var matchingSource = packageSources.FirstOrDefault (currentSource => currentSource.Equals (source));
			return matchingSource?.IsEnabled == true;
		}

		public IEnumerable<PackageSource> LoadPackageSources ()
		{
			return packageSources;
		}

		public void SaveActivePackageSource (PackageSource source)
		{
		}

		public void SavePackageSources (IEnumerable<PackageSource> sources)
		{
		}

		public void UpdatePackageSources (Protocol.PackageSource[] sources)
		{
			packageSources = sources.Select (source => source.ToNuGetPackageSource ()).ToList ();
			PackageSourcesChanged?.Invoke (this, EventArgs.Empty);
		}
	}
}

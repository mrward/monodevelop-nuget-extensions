//
// ConsoleHostSourceRepositoryProvider.cs
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

using System.Collections.Generic;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.VisualStudio;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core
{
	public class ConsoleHostSourceRepositoryProvider : ISourceRepositoryProvider
	{
		ConsoleHostPackageSourceProvider packageSourceProvider;
		SourceRepositoryProvider sourceRepositoryProvider;

		public ConsoleHostSourceRepositoryProvider ()
		{
			packageSourceProvider = new ConsoleHostPackageSourceProvider ();
			sourceRepositoryProvider = new SourceRepositoryProvider (PackageSourceProvider, Repository.Provider.GetVisualStudio ());
		}

		public IPackageSourceProvider PackageSourceProvider => packageSourceProvider;

		public SourceRepository CreateRepository (PackageSource source)
		{
			return sourceRepositoryProvider.CreateRepository (source);
		}

		public SourceRepository CreateRepository (PackageSource source, FeedType type)
		{
			return sourceRepositoryProvider.CreateRepository (source, type);
		}

		public IEnumerable<SourceRepository> GetRepositories ()
		{
			return sourceRepositoryProvider.GetRepositories ();
		}

		public void UpdatePackageSources (Protocol.PackageSource[] sources)
		{
			packageSourceProvider.UpdatePackageSources (sources);
		}
	}
}

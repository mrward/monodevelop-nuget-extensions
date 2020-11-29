// 
// PackageManagementConsoleHostProvider.cs
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

using MonoDevelop.DotNetCore;

namespace MonoDevelop.PackageManagement.Scripting
{
	internal class PackageManagementConsoleHostProvider
	{
		public static DotNetCoreVersion RequiredDotNetCoreRuntimeVersion { get; } = new DotNetCoreVersion (3, 1, 0);

		IPackageManagementConsoleHost consoleHost;
		IPackageManagementEvents packageEvents;
		
		public PackageManagementConsoleHostProvider ()
			: this (
				PackageManagementServices.PackageManagementEvents)
		{
		}

		public PackageManagementConsoleHostProvider (
			IPackageManagementEvents packageEvents)
		{
			this.packageEvents = packageEvents;
		}
		
		public IPackageManagementConsoleHost ConsoleHost {
			get {
				if (consoleHost == null) {
					CreateConsoleHost ();
				}
				return consoleHost;
			}
		}

		void CreateConsoleHost ()
		{
			if (IsSupportedDotNetCoreRuntimeInstalled ()) {
				consoleHost = new PackageManagementConsoleHost (packageEvents);
			} else {
				consoleHost = new DotNetCoreRuntimeMissingConsoleHost (RequiredDotNetCoreRuntimeVersion);
			}
		}

		static bool IsSupportedDotNetCoreRuntimeInstalled ()
		{
			if (!DotNetCoreRuntime.IsInstalled)
				return false;

			foreach (DotNetCoreVersion version in DotNetCoreRuntimeVersions.GetInstalledVersions (DotNetCoreRuntime.FileName)) {
				if (RequiredDotNetCoreRuntimeVersion.Major == version.Major &&
					RequiredDotNetCoreRuntimeVersion.Minor == version.Minor) {
					return true;
				}
			}

			return false;
		}
	}
}

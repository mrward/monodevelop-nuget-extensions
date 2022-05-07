//
// PackageManagementConsoleHostServiceProvider.cs
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
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using MonoDevelop.PackageManagement.VisualStudio;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Protocol.Core.Types;
using NuGetConsole;

namespace MonoDevelop.PackageManagement.Scripting
{
	class PackageManagementConsoleHostServiceProvider : IServiceProvider
	{
		readonly PackageManagementConsoleHost consoleHost;

		Dictionary<Type, object> services = new Dictionary<Type, object> ();

		public PackageManagementConsoleHostServiceProvider (PackageManagementConsoleHost consoleHost)
		{
			this.consoleHost = consoleHost;
		}

		public object GetService (Type serviceType)
		{
			lock (services) {
				if (services.TryGetValue (serviceType, out object instance)) {
					return instance;
				}

				if (serviceType == typeof (ISourceRepositoryProvider)) {
					return consoleHost.SolutionManager.CreateSourceRepositoryProvider ();
				} else if (serviceType == typeof (IConsoleHostSolutionManager)) {
					return consoleHost.SolutionManager;
				//} else if (serviceType == typeof (SVsExtensionManager)) {
				//	return new SVsExtensionManager ();
				} else if (serviceType == typeof (IPowerConsoleWindow)) {
					return new PowerConsoleToolWindow ();
				//if (type.FullName == typeof (IConsoleInitializer).FullName) {
				//	return new ConsoleInitializer (GetConsoleHost ());
				//} else if (type.FullName == typeof (IVsPackageInstallerServices).FullName) {
				//	return new VsPackageInstallerServices (GetSolution ());
				//}
				} else if (serviceType == typeof (SComponentModel)) {
					return new ComponentModel ();
				} else if (serviceType == typeof (IConsoleHostNuGetPackageManager)) {
					return consoleHost.CreatePackageManager ();
				} else if (serviceType == typeof (ISettings)) {
					return consoleHost.Settings;
				} else if (serviceType == typeof (ICommonOperations)) {
					return new MonoDevelopCommonOperations ();
				} else if (serviceType == typeof (IVsSolution) ||
					serviceType == typeof (SVsSolution)) {
					return new VsSolution ();
				}
			}
			return null;
		}

		public void AddService (Type serviceType, object instance)
		{
			lock (services) {
				services [serviceType] = instance;
			}
		}
	}
}

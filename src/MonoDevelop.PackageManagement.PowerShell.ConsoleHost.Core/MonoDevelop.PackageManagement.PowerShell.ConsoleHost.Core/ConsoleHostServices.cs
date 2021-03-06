﻿//
// ConsoleHostServices.cs
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

using System.Collections.Concurrent;
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;
using NuGet.PackageManagement.PowerShellCmdlets;
using NuGet.Protocol.Core.Types;
using NuGet.VisualStudio;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core
{
	public static class ConsoleHostServices
	{
		public static void Initialize (DTE dte)
		{
			DTE = dte;
			SourceRepositoryProvider = new ConsoleHostSourceRepositoryProvider ();
			SolutionManager = new ConsoleHostSolutionManager ();

			var serviceProvider = new DefaultPackageServiceProvider ();
			serviceProvider.AddService (typeof (DTE), dte);
			serviceProvider.AddService (typeof (ISourceRepositoryProvider), SourceRepositoryProvider);
			serviceProvider.AddService (typeof (IConsoleHostSolutionManager), SolutionManager);

			ServiceLocator.InitializePackageServiceProvider (serviceProvider);
		}

		public static ConsoleHostSourceRepositoryProvider SourceRepositoryProvider { get; private set; }

		public static DTE DTE { get; private set; }

		public static ConsoleHostSolutionManager SolutionManager { get; private set; }

		public static BlockingCollection<Message> ActiveBlockingCollection { get; set; }
	}
}

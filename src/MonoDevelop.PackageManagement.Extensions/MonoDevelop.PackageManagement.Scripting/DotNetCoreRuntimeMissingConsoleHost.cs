//
// DotNetCoreRuntimeMissingConsoleHost.cs
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
using System.Threading;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGetConsole;
using FileConflictAction = NuGet.ProjectManagement.FileConflictAction;

namespace MonoDevelop.PackageManagement.Scripting
{
	class DotNetCoreRuntimeMissingConsoleHost : IPackageManagementConsoleHost
	{
		RegisteredPackageSources registeredPackageSources;
		ConsoleHostSolutionManager solutionManager;

		public DotNetCoreRuntimeMissingConsoleHost ()
		{
			solutionManager = new ConsoleHostSolutionManager ();
			registeredPackageSources = new RegisteredPackageSources (solutionManager);
		}

		public Project DefaultProject { get; set; }

		public IScriptingConsole ScriptingConsole { get; set; }
		public ISettings Settings { get; }
		public bool IsRunning { get; }
		public bool IsSolutionOpen { get; }
		public CancellationToken Token { get; }
		public IScriptExecutor ScriptExecutor { get; }
		public ICommandExpansion CommandExpansion { get; }

		#pragma warning disable 67
		public event EventHandler RunningCommand;
		public event EventHandler CommandCompleted;
		#pragma warning restore 67

		public IMonoDevelopSolutionManager SolutionManager {
			get { return solutionManager; }
		}

		public SourceRepositoryViewModel ActivePackageSource {
			get { return registeredPackageSources.SelectedPackageSource; }
			set { registeredPackageSources.SelectedPackageSource = value; }
		}

		public IEnumerable<SourceRepositoryViewModel> PackageSources {
			get { return registeredPackageSources.PackageSources; }
		}

		public ConsoleHostNuGetPackageManager CreatePackageManager ()
		{
			return null;
		}

		public void Dispose ()
		{
		}

		public void Clear ()
		{
		}

		public void WritePrompt ()
		{
		}

		public void Run ()
		{
			string message = GetDotNetCoreRuntimeIsNotInstalledMessage ();
			ScriptingConsole.WriteLine (message, ScriptingStyle.Error);
		}

		string GetDotNetCoreRuntimeIsNotInstalledMessage ()
		{
			return
				GettextCatalog.GetString (
				"The .NET Core runtime is not installed.\r\n" +
				"The NuGet Package Manager Console requires the .NET Core 2.1 runtime.\r\n" +
				"The .NET Core 2.1 runtime can be downloaded from https://dotnet.microsoft.com/download/dotnet-core/2.1");
		}

		public void ShutdownConsole ()
		{
		}

		public void ExecuteCommand (string command)
		{
		}

		public void ProcessUserInput (string line)
		{
		}

		public void StopCommand ()
		{
		}

		public IConsoleHostFileConflictResolver CreateFileConflictResolver (FileConflictAction? fileConflictAction)
		{
			return null;
		}

		public IDisposable CreateEventsMonitor (INuGetProjectContext context)
		{
			return null;
		}

		public string GetActivePackageSource (string source)
		{
			return null;
		}

		public IEnumerable<NuGetProject> GetNuGetProjects ()
		{
			return Enumerable.Empty<NuGetProject> ();
		}

		public NuGetProject GetNuGetProject (string projectName)
		{
			return null;
		}

		public IEnumerable<PackageSource> LoadPackageSources ()
		{
			return Enumerable.Empty<PackageSource> ();
		}

		public SourceRepository CreateRepository (PackageSource source)
		{
			return null;
		}

		public IEnumerable<SourceRepository> GetRepositories ()
		{
			return Enumerable.Empty<SourceRepository> ();
		}

		public void ReloadPackageSources ()
		{
		}

		public void OnSolutionUnloaded ()
		{
		}

		public void OnMaxVisibleColumnsChanged ()
		{
		}
	}
}

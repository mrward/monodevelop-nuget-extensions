// 
// PackageManagementConsoleHost.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGetConsole;
using NuGetConsole.Host.PowerShell;
using NuGetConsole.Host;

namespace MonoDevelop.PackageManagement.Scripting
{
	internal class PackageManagementConsoleHost : IPackageManagementConsoleHost
	{
		RegisteredPackageSources registeredPackageSources;
		IPowerShellHostFactory powerShellHostFactory;
		IPowerShellHost powerShellHost;
		IMonoDevelopSolutionManager solutionManager;
		ISourceRepositoryProvider sourceRepositoryProvider;
		IPackageManagementAddInPath addinPath;
		IPackageManagementEvents packageEvents;
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource ();
		Project defaultProject;
		Lazy<IScriptExecutor> scriptExecutor;
		LazyCommandExpansion commandExpansion;
		string prompt = "PM> ";

		public PackageManagementConsoleHost (
			IPackageManagementEvents packageEvents,
			IMonoDevelopSolutionManager solutionManager,
			IPowerShellHostFactory powerShellHostFactory,
			IPackageManagementAddInPath addinPath)
		{
			this.registeredPackageSources = new RegisteredPackageSources (solutionManager);
			this.powerShellHostFactory = powerShellHostFactory;
			this.solutionManager = solutionManager;
			this.addinPath = addinPath;
			this.packageEvents = packageEvents;

			InitializeLazyScriptExecutor ();

			commandExpansion = new LazyCommandExpansion (CreateCommandExpansion);
		}

		private void InitializeLazyScriptExecutor ()
		{
			scriptExecutor = new Lazy<IScriptExecutor> (() => CreateScriptExecutor ());
		}

		public PackageManagementConsoleHost (
			IPackageManagementEvents packageEvents)
			: this (
				packageEvents,
				new ConsoleHostSolutionManager (),
				new RemotePowerShellHostFactory (),
				new PackageManagementAddInPath ())
		{
		}

		public bool IsRunning { get; private set; }

		public event EventHandler CommandCompleted;
		public event EventHandler RunningCommand;

		public Project DefaultProject {
			get { return defaultProject; }
			set {
				if (defaultProject != value) {
					defaultProject = value;
					powerShellHost?.OnDefaultProjectChanged (defaultProject);
				}
			}
		}

		public SourceRepositoryViewModel ActivePackageSource {
			get { return registeredPackageSources.SelectedPackageSource; }
			set {
				registeredPackageSources.SelectedPackageSource = value;
				powerShellHost?.OnActiveSourceChanged (value);
			}
		}

		public IEnumerable<SourceRepositoryViewModel> PackageSources {
			get { return registeredPackageSources.PackageSources; }
		}

		public IScriptingConsole ScriptingConsole { get; set; }

		public bool IsSolutionOpen {
			get { return solutionManager.IsSolutionOpen; }
		}

		public CancellationToken Token {
			get { return cancellationTokenSource.Token; }
		}

		public IMonoDevelopSolutionManager SolutionManager {
			get { return solutionManager; }
		}

		public ISettings Settings {
			get { return solutionManager.Settings; }
		}

		public void Dispose ()
		{
			//			ShutdownConsole();
			//			
			//			if (thread != null) {
			//				if (thread.Join(100)) {
			//					thread = null;
			//					IsRunning = false;
			//				}
			//			}
		}

		public void Clear ()
		{
			ScriptingConsole.Clear ();
		}

		public void WritePrompt ()
		{
			ScriptingConsole.Write (prompt, ScriptingStyle.Prompt);
		}

		public void Run ()
		{
			PackageManagementBackgroundDispatcher.Dispatch (() => {
				RunSynchronous ();
				IsRunning = true;
			});
		}

		void RunSynchronous ()
		{
			InitPowerShell ();
			WriteInfoBeforeFirstPrompt ();
			InitializePackageScriptsForOpenSolution ();
			WritePrompt ();

			IdeApp.Workspace.SolutionLoaded += SolutionLoaded;
		}

		void InitPowerShell ()
		{
			CreatePowerShellHost ();
			AddModulesToImport ();
			UpdateWorkingDirectory ();
			ConfigurePackageSources ();
			OnMaxVisibleColumnsChanged ();
		}

		void ConfigurePackageSources ()
		{
			powerShellHost?.OnPackageSourcesChanged (PackageSources, ActivePackageSource);
		}

		void CreatePowerShellHost ()
		{
			var clearConsoleHostCommand = new ClearPackageManagementConsoleHostCommand (this);
			powerShellHost =
				powerShellHostFactory.CreatePowerShellHost (
					this.ScriptingConsole,
					GetNuGetVersion (),
					clearConsoleHostCommand,
					new ICSharpCode.PackageManagement.EnvDTE.DTE ());

			powerShellHost.Exited += PowerShellHostExited;
		}

		protected virtual Version GetNuGetVersion ()
		{
			return NuGetVersion.Version;
		}

		void AddModulesToImport ()
		{
			string module = addinPath.CmdletsAssemblyFileName;
			powerShellHost.ModulesToImport.Add (module);
		}

		void WriteInfoBeforeFirstPrompt ()
		{
			WriteNuGetVersionInfo ();
			WriteHelpInfo ();
			WriteLine ();
		}

		void WriteNuGetVersionInfo ()
		{
			string versionInfo = String.Format ("NuGet {0}", powerShellHost.Version);
			WriteLine (versionInfo);
		}

		void UpdateWorkingDirectory ()
		{
			string directory = GetWorkingDirectory ();
			UpdateWorkingDirectory (directory);
		}

		string GetWorkingDirectory ()
		{
			var workingDirectory = new PowerShellWorkingDirectory (PackageManagementExtendedServices.ProjectService);
			return workingDirectory.GetWorkingDirectory ();
		}

		void UpdateWorkingDirectory (Solution solution)
		{
			string directory = PowerShellWorkingDirectory.QuotedDirectory (solution.BaseDirectory);
			UpdateWorkingDirectory (directory);
		}

		void UpdateWorkingDirectory (string directory)
		{
			if (powerShellHost == null)
				return;

			string command = String.Format ("Set-Location {0}", directory);

			PackageManagementBackgroundDispatcher.Dispatch (() => {
				powerShellHost.ExecuteCommand (command);
			});
		}

		void InitializePackageScriptsForOpenSolution ()
		{
			var solution = PackageManagementServices.ProjectService.OpenSolution?.Solution;
			if (solution != null) {
				UpdateWorkingDirectory (solution);
				powerShellHost?.SolutionLoaded (solution);
				powerShellHost?.OnDefaultProjectChanged (DefaultProject);
				RunPowerShellInitializationScripts (solution);
			} else {
				UpdateWorkingDirectory ();
			}
		}

		void WriteLine (string message)
		{
			ScriptingConsole.WriteLine (message, ScriptingStyle.Out);
		}

		void WriteLine ()
		{
			WriteLine (String.Empty);
		}

		void WriteHelpInfo ()
		{
			string helpInfo = GetHelpInfo ();
			WriteLine (helpInfo);
		}

		protected virtual string GetHelpInfo ()
		{
			return "Type 'get-help NuGet' for more information.";
		}

		public void ProcessUserInput (string line)
		{
			OnRunningCommand ();
			InitializeToken ();

			PackageManagementBackgroundDispatcher.Dispatch (() => {
				powerShellHost.ExecuteCommand (line);
				OnCommandCompleted ();
				WritePrompt ();
			});
		}

		void InitializeToken ()
		{
			if (cancellationTokenSource.IsCancellationRequested) {
				cancellationTokenSource.Dispose ();
				cancellationTokenSource = new CancellationTokenSource ();
			}
		}

		public string GetActivePackageSource (string source)
		{
			if (source != null) {
				return source;
			}
			return ActivePackageSource?.PackageSource?.Name;
		}

		string GetActiveProjectName (string projectName)
		{
			if (projectName != null) {
				return projectName;
			}
			return DefaultProject.Name;
		}

		public void ShutdownConsole ()
		{
			//			if (ScriptingConsole != null) {
			//				ScriptingConsole.Dispose();
			//			}
		}

		public void ExecuteCommand (string command)
		{
			PackageManagementBackgroundDispatcher.Dispatch (() => {
				powerShellHost.ExecuteCommand (command);
				WritePrompt ();
			});
		}

		public IConsoleHostFileConflictResolver CreateFileConflictResolver (FileConflictAction? fileConflictAction)
		{
			return new ConsoleHostFileConflictResolver (packageEvents, fileConflictAction);
		}

		public IDisposable CreateEventsMonitor (INuGetProjectContext context)
		{
			return new ConsoleHostPackageEventsMonitor (context, packageEvents);
		}

		public IEnumerable<NuGetProject> GetNuGetProjects ()
		{
			var task = solutionManager.GetNuGetProjectsAsync ();
			task.Wait ();
			return task.Result.ToList ();
		}

		public NuGetProject GetNuGetProject (string projectName)
		{
			string activeProjectName = GetActiveProjectName (projectName);
			var task = solutionManager.GetNuGetProjectAsync (activeProjectName);
			task.Wait ();
			return task.Result;
		}

		public IEnumerable<PackageSource> LoadPackageSources ()
		{
			sourceRepositoryProvider = solutionManager.CreateSourceRepositoryProvider ();
			return sourceRepositoryProvider.PackageSourceProvider?.LoadPackageSources ();
		}

		public SourceRepository CreateRepository (PackageSource source)
		{
			return sourceRepositoryProvider?.CreateRepository (source);
		}

		public IEnumerable<SourceRepository> GetRepositories ()
		{
			if (sourceRepositoryProvider != null)
				return sourceRepositoryProvider.GetRepositories ();

			return Enumerable.Empty<SourceRepository> ();
		}

		public void ReloadPackageSources ()
		{
			registeredPackageSources.ReloadSettings ();
			if (powerShellHost != null) {
				var sources = registeredPackageSources.PackageSources.ToList ();
				powerShellHost.OnPackageSourcesChanged (sources, registeredPackageSources.SelectedPackageSource);
			}
		}

		public ConsoleHostNuGetPackageManager CreatePackageManager ()
		{
			var consoleHostSolutionManager = (ConsoleHostSolutionManager)solutionManager;
			return new ConsoleHostNuGetPackageManager (consoleHostSolutionManager.GetMonoDevelopSolutionManager ());
		}

		public void OnSolutionUnloaded ()
		{
			UpdateWorkingDirectory ();
			powerShellHost?.SolutionUnloaded ();
			ScriptExecutor.Reset ();
		}

		void SolutionLoaded (object sender, SolutionEventArgs e)
		{
			UpdateWorkingDirectory (e.Solution);
			powerShellHost?.SolutionLoaded (e.Solution);
			RunPowerShellInitializationScripts (e.Solution);
		}

		void RunPowerShellInitializationScripts (Solution solution)
		{
			OnRunningCommand ();
			InitializeToken ();

			PackageManagementBackgroundDispatcher.Dispatch (() => {
				bool showPrompt = SafeRunPowerShellInitializationScripts (solution);

				OnCommandCompleted ();

				if (showPrompt) {
					WritePrompt ();
				}
			});
		}

		bool SafeRunPowerShellInitializationScripts (Solution solution)
		{
			try {
				var runner = new InitializationScriptRunner (solution, ScriptExecutor);
				return runner.ExecuteInitScriptsAsync (Token).WaitAndGetResult ();
			} catch (Exception ex) {
				LoggingService.LogError ("SafeRunPowerShellInitializationScripts error", ex);
				ScriptingConsole.WriteLine ("\n" + ex.Message, ScriptingStyle.Error);
				return true;
			}
		}

		public void OnMaxVisibleColumnsChanged ()
		{
			if (powerShellHost != null) {
				int columns = ScriptingConsole.GetMaximumVisibleColumns ();
				powerShellHost.OnMaxVisibleColumnsChanged (columns);
			}
		}

		void OnCommandCompleted ()
		{
			Runtime.RunInMainThread (() => {
				CommandCompleted?.Invoke (this, EventArgs.Empty);
			}).WaitAndGetResult ();
		}

		void OnRunningCommand ()
		{
			Runtime.RunInMainThread (() => {
				RunningCommand?.Invoke (this, EventArgs.Empty);
			}).WaitAndGetResult ();
		}

		public void StopCommand ()
		{
			try {
				cancellationTokenSource.Cancel ();
				powerShellHost?.StopCommand ();
			} catch (Exception ex) {
				LoggingService.LogError ("StopCommand error.", ex);
			}
		}

		public IScriptExecutor ScriptExecutor {
			get { return scriptExecutor.Value; }
		}

		IScriptExecutor CreateScriptExecutor ()
		{
			return powerShellHost.CreateScriptExecutor ();
		}

		public ICommandExpansion CommandExpansion {
			get { return commandExpansion; }
		}

		ICommandExpansion CreateCommandExpansion ()
		{
			ITabExpansion tabExpansion = powerShellHost.CreateTabExpansion ();
			return new CommandExpansion (tabExpansion);
		}

		void PowerShellHostExited (object sender, EventArgs e)
		{
			try {
				ReloadPackageSources ();
				OnMaxVisibleColumnsChanged ();
				InitializeLazyScriptExecutor ();
				commandExpansion.CommandExpansion = CreateCommandExpansion ();
				InitializePackageScriptsForOpenSolution ();
			} catch (Exception ex) {
				string message = GettextCatalog.GetString ("Unable to initialize remote PowerShell host. {0}", ex.Message);
				ScriptingConsole.WriteLine (message, ScriptingStyle.Error);
				LoggingService.LogError ("PowerShellHostExited error", ex);
			}
		}
	}
}

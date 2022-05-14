// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement;
using MonoDevelop.PackageManagement.Scripting;
using MonoDevelop.Projects;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;

namespace NuGetConsole
{
	internal class ScriptExecutor : IScriptExecutor
	{
		ConcurrentDictionary<PackageIdentity, PackageInitPS1State> InitScriptExecutions
			= new ConcurrentDictionary<PackageIdentity, PackageInitPS1State> (PackageIdentityComparer.Default);

		IPowerShellHost host;
		Lazy<ISettings> Settings { get; set; }

		public ScriptExecutor (IPowerShellHost host)
		{
			this.host = host;
			Settings = new Lazy<ISettings> (() => SettingsLoader.LoadDefaultSettings ());
			Reset ();
		}

		public void Reset ()
		{
			InitScriptExecutions.Clear ();
		}

		public async Task<bool> ExecuteAsync (
			PackageIdentity identity,
			string installPath,
			string relativeScriptPath,
			DotNetProject project,
			INuGetProjectContext nuGetProjectContext,
			bool throwOnFailure)
		{
			var scriptPath = Path.Combine (installPath, relativeScriptPath);

			if (File.Exists (scriptPath)) {
				if (scriptPath.EndsWith (PowerShellScripts.Init, StringComparison.OrdinalIgnoreCase)
					&& !TryMarkVisited (identity, PackageInitPS1State.FoundAndExecuted)) {
					return true;
				}

				var dteProject = MonoDevelop.PackageManagement.EnvDTE.ProjectFactory.CreateProject (project);
				var request = new ScriptExecutionRequest (scriptPath, installPath, identity, dteProject);

				var psNuGetProjectContext = nuGetProjectContext as IPSNuGetProjectContext;
				if (psNuGetProjectContext != null
					&& psNuGetProjectContext.IsExecuting
					&& psNuGetProjectContext.CurrentPSCmdlet != null) {
					var psVariable = psNuGetProjectContext.CurrentPSCmdlet.SessionState.PSVariable;

					// set temp variables to pass to the script
					psVariable.Set ("__rootPath", request.InstallPath);
					psVariable.Set ("__toolsPath", request.ToolsPath);
					psVariable.Set ("__package", request.ScriptPackage);
					psVariable.Set ("__project", request.Project);

					psNuGetProjectContext.ExecutePSScript (request.ScriptPath, throwOnFailure);
				} else {
					var logMessage = string.Format (CultureInfo.CurrentCulture, "Executing script file '{0}'...", scriptPath);
					// logging to both the Output window and progress window.
					nuGetProjectContext.Log (MessageLevel.Info, logMessage);
					try {
						await ExecuteScriptCoreAsync (request);
					} catch (Exception ex) {
						// throwFailure is set by Package Manager.
						if (throwOnFailure) {
							throw;
						}
						nuGetProjectContext.Log (MessageLevel.Warning, ex.Message);
					}
				}

				return true;
			} else {
				if (scriptPath.EndsWith (PowerShellScripts.Init, StringComparison.OrdinalIgnoreCase)) {
					TryMarkVisited (identity, PackageInitPS1State.NotFound);
				}
			}
			return false;
		}

		public bool TryMarkVisited (PackageIdentity packageIdentity, PackageInitPS1State initPS1State)
		{
			return InitScriptExecutions.TryAdd (packageIdentity, initPS1State);
		}

		/// <summary>
		/// This is not used.
		/// </summary>
		public Task<bool> ExecuteInitScriptAsync (PackageIdentity identity, CancellationToken token)
		{
			throw new NotImplementedException ("ScriptExecutor.ExecuteInitScriptAsync is not implemented");
		}

		async Task ExecuteScriptCoreAsync (ScriptExecutionRequest request)
		{
			// Host.Execute calls powershell's pipeline.Invoke and blocks the calling thread
			// to switch to powershell pipeline execution thread. In order not to block the UI thread,
			// go off the UI thread. This is important, since, switches to UI thread,
			// using SwitchToMainThreadAsync will deadlock otherwise
			await Task.Run (() => host.ExecuteCommand (request.BuildCommand (), request.BuildInput ()));
		}
	}
}
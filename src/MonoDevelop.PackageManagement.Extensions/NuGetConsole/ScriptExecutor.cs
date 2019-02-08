// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.PackageManagement.Protocol;
using MonoDevelop.Projects;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using StreamJsonRpc;

namespace NuGetConsole
{
	internal class ScriptExecutor : IScriptExecutor
	{
		ConcurrentDictionary<PackageIdentity, PackageInitPS1State> InitScriptExecutions
			= new ConcurrentDictionary<PackageIdentity, PackageInitPS1State> (PackageIdentityComparer.Default);

		Lazy<ISettings> Settings { get; set; }
		JsonRpc rpc;

		public ScriptExecutor (JsonRpc rpc)
		{
			this.rpc = rpc;
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
			bool throwOnFailure,
			CancellationToken token)
		{
			var scriptPath = Path.Combine (installPath, relativeScriptPath);

			if (File.Exists (scriptPath)) {
				if (scriptPath.EndsWith (PowerShellScripts.Init, StringComparison.OrdinalIgnoreCase)
					&& !TryMarkVisited (identity, PackageInitPS1State.FoundAndExecuted)) {
					return true;
				}

				var consoleNuGetProjectContext = nuGetProjectContext as ConsoleHostNuGetProjectContext;
				if (consoleNuGetProjectContext != null &&
					consoleNuGetProjectContext.IsExecutingPowerShellCommand) {
					var message = new RunScriptParams {
						ScriptPath = scriptPath,
						InstallPath = installPath,
						PackageId = identity.Id,
						PackageVersion = identity.Version.ToNormalizedString (),
						Project = project.CreateProjectInformation (),
						ThrowOnFailure = throwOnFailure
					};
					await RunScriptWithRemotePowerShellConsoleHost (message, nuGetProjectContext, throwOnFailure, token);
				} else {
					var logMessage = GettextCatalog.GetString ("Executing script file '{0}'...", scriptPath);
					nuGetProjectContext.Log (MessageLevel.Info, logMessage);

					var message = new RunInitScriptParams {
						ScriptPath = scriptPath,
						InstallPath = installPath,
						PackageId = identity.Id,
						PackageVersion = identity.Version.ToNormalizedString (),
						ThrowOnFailure = throwOnFailure
					};
					await RunInitScriptWithRemotePowerShellConsoleHost (message, nuGetProjectContext, throwOnFailure, token);
				}

				return true;
			} else {
				if (scriptPath.EndsWith (PowerShellScripts.Init, StringComparison.OrdinalIgnoreCase)) {
					TryMarkVisited (identity, PackageInitPS1State.NotFound);
				}
			}
			return false;
		}

		async Task RunScriptWithRemotePowerShellConsoleHost (
			RunScriptParams message,
			INuGetProjectContext nuGetProjectContext,
			bool throwOnFailure,
			CancellationToken token)
		{
			await RunScriptWithRemotePowerShellConsoleHost (Methods.RunScript, message, nuGetProjectContext, throwOnFailure, token);
		}

		async Task RunInitScriptWithRemotePowerShellConsoleHost (
			RunInitScriptParams message,
			INuGetProjectContext nuGetProjectContext,
			bool throwOnFailure,
			CancellationToken token)
		{
			await RunScriptWithRemotePowerShellConsoleHost (Methods.RunInitScript, message, nuGetProjectContext, throwOnFailure, token);
		}

		async Task RunScriptWithRemotePowerShellConsoleHost (
			string method,
			object message,
			INuGetProjectContext nuGetProjectContext,
			bool throwOnFailure,
			CancellationToken token)
		{
			try {
				var result = await rpc.InvokeWithCancellationAsync<RunScriptResult> (method, new[] { message }, token);
				if (!result.Success) {
					throw new ApplicationException (result.ErrorMessage);
				}
			} catch (Exception ex) {
				if (throwOnFailure) {
					throw;
				} else {
					nuGetProjectContext.Log (MessageLevel.Warning, ex.Message);
				}
			}
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
	}
}
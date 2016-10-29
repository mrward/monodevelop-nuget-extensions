// 
// PackageManagementCmdlet.cs
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
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using NuGet;
using NuGet.PackageManagement.UI;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;

using MessageLevel = NuGet.MessageLevel;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	public abstract class PackageManagementCmdlet : PSCmdlet, ITerminatingCmdlet, IPackageScriptSession, IPackageScriptRunner, ILogger
	{
		IPackageManagementConsoleHost consoleHost;
		ICmdletTerminatingError terminatingError;
		SourceRepository activeSourceRepository;

		internal PackageManagementCmdlet(
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
		{
			this.consoleHost = consoleHost;
			this.terminatingError = terminatingError;
		}
		
		internal IPackageManagementConsoleHost ConsoleHost {
			get { return consoleHost; }
		}
		
		protected Project DefaultProject {
			get { return consoleHost.DefaultProject as Project; }
		}

		protected ICmdletTerminatingError TerminatingError {
			get {
				if (terminatingError == null) {
					terminatingError = new CmdletTerminatingError(this);
				}
				return terminatingError;
			}
		}
		
		protected void ThrowErrorIfProjectNotOpen()
		{
			if (DefaultProject == null) {
				ThrowProjectNotOpenTerminatingError();
			}
		}
			
		protected void ThrowProjectNotOpenTerminatingError()
		{
			TerminatingError.ThrowNoProjectOpenError();
		}
		
		public void SetEnvironmentPath(string path)
		{
			SetSessionVariable("env:path", path);
		}
		
		protected virtual void SetSessionVariable(string name, object value)
		{
			SessionState.PSVariable.Set(name, value);
		}
		
		public string GetEnvironmentPath()
		{
			return (string)GetSessionVariable("env:path");
		}
		
		protected virtual object GetSessionVariable(string name)
		{
			return GetVariableValue(name);
		}
		
		public void AddVariable(string name, object value)
		{
			SetSessionVariable(name, value);
		}
		
		public void RemoveVariable(string name)
		{
			RemoveSessionVariable(name);
		}
		
		protected virtual void RemoveSessionVariable(string name)
		{
			SessionState.PSVariable.Remove(name);
		}
		
		public virtual void InvokeScript(string script)
		{
			var resultTypes = PipelineResultTypes.Error | PipelineResultTypes.Output;
			try {
				InvokeCommand.InvokeScript(script, false, resultTypes, null, null);
			} catch (Exception ex) {
				LoggingService.LogInternalError ("PowerShell script error.", ex);
				var errorRecord = new ErrorRecord (ex,
					"PackageManagementInternalError",
					ErrorCategory.InvalidOperation,
					null);
				WriteError (errorRecord);
			}
		}
		
		void IPackageScriptRunner.Run(IPackageScript script)
		{
			if (script.Exists()) {
				script.Run(this);
			}
		}

		protected IDisposable CreateEventsMonitor ()
		{
			return ConsoleHost.CreateEventsMonitor (this);
		}

//		internal void ExecuteWithScriptRunner (IPackageManagementProject2 project, Action action)
//		{
//			using (RunPackageScriptsAction runScriptsAction = CreateRunPackageScriptsAction (project)) {
//				action ();
//			}
//		}
//
//		RunPackageScriptsAction CreateRunPackageScriptsAction (IPackageManagementProject2 project)
//		{
//			return new RunPackageScriptsAction (this, project);
//		}

		public void Log (MessageLevel level, string message, params object[] args)
		{
			string fullMessage = String.Format (message, args);

			switch (level) {
				case MessageLevel.Error:
					WriteError (CreateErrorRecord (message));
					break;
				case MessageLevel.Warning:
					WriteWarning (fullMessage);
					break;
				case MessageLevel.Debug:
					WriteVerbose (fullMessage);
					break;
				default:
					Host.UI.WriteLine (message);
					break;
			}
		}

		ErrorRecord CreateErrorRecord (string message)
		{
			return new ErrorRecord (
				new Exception (message),
				"PackageManagementErrorId",
				ErrorCategory.NotSpecified,
				null);
		}

		public FileConflictResolution ResolveFileConflict (string message)
		{
			throw new NotImplementedException ();
		}

		protected void CheckSolutionIsOpen ()
		{
			if (!ConsoleHost.IsSolutionOpen) {
				ThrowSolutionNotOpenTerminatingError ();
				TerminatingError.ThrowNoProjectOpenError();
			}
		}

		void ThrowSolutionNotOpenTerminatingError()
		{
			TerminatingError.ThrowNoSolutionOpenError ();
		}

		protected IEnumerable<SourceRepository> EnabledSourceRepositories { get; private set; }

		/// <summary>
		/// Initializes source repositories for PowerShell cmdlets, based on config, source string, and/or host active source property value.
		/// </summary>
		/// <param name="source">The source string specified by -Source switch.</param>
		protected void UpdateActiveSourceRepository (string source)
		{
			var packageSources = ConsoleHost.LoadPackageSources ();

			// If source string is not specified, get the current active package source from the host
			source = ConsoleHost.GetActivePackageSource (source);

			if (!string.IsNullOrEmpty (source)) {
				// Look through all available sources (including those disabled) by matching source name and url
				var matchingSource = packageSources
					?.Where (p => StringComparer.OrdinalIgnoreCase.Equals (p.Name, source) ||
					 	StringComparer.OrdinalIgnoreCase.Equals (p.Source, source))
				     .FirstOrDefault ();

				if (matchingSource != null) {
					activeSourceRepository = ConsoleHost.CreateRepository (matchingSource);
				} else {
					// source should be the format of url here; otherwise it cannot resolve from name anyways.
					activeSourceRepository = CreateRepositoryFromSource (source);
				}
			}

			EnabledSourceRepositories = ConsoleHost
				.GetRepositories ()
				.Where (r => r.PackageSource.IsEnabled)
				.ToList ();
		}

		/// <summary>
		/// Create a package repository from the source by trying to resolve relative paths.
		/// </summary>
		protected SourceRepository CreateRepositoryFromSource (string source)
		{
			if (source == null) {
				throw new ArgumentNullException (nameof (source));
			}

			var packageSource = new NuGet.Configuration.PackageSource (source);
			var repository = ConsoleHost.CreateRepository (packageSource);
			var resource = repository.GetResource<PackageSearchResource> ();

			// resource can be null here for relative path package source.
			if (resource == null) {
				Uri uri;
				// if it's not an absolute path, treat it as relative path
				if (Uri.TryCreate (source, UriKind.Relative, out uri)) {
					throw new NotImplementedException ();
					//string outputPath;
					//bool? exists;
					//string errorMessage;
					//// translate relative path to absolute path
					//if (TryTranslatePSPath (source, out outputPath, out exists, out errorMessage) && exists == true) {
					//	source = outputPath;
					//	packageSource = new Configuration.PackageSource (outputPath);
					//}
				}
			}

			var sourceRepo = ConsoleHost.CreateRepository (packageSource);
			// Right now if packageSource is invalid, CreateRepository will not throw. Instead, resource returned is null.
			var newResource = repository.GetResource<PackageSearchResource> ();
			if (newResource == null) {
				// Try to create Uri again to throw UriFormat exception for invalid source input.
				new Uri (source);
			}
			return sourceRepo;
		}

		protected static async Task<Dictionary<NuGetProject, IEnumerable<NuGet.Packaging.PackageReference>>> GetInstalledPackagesAsync(
			IEnumerable<NuGetProject> projects,
			string filter,
			int skip,
			int take,
			CancellationToken token)
		{
			var installedPackages = new Dictionary<NuGetProject, IEnumerable<NuGet.Packaging.PackageReference>>();

			foreach (var project in projects) {
				var packageRefs = await project.GetInstalledPackagesAsync (token);

				if (!string.IsNullOrEmpty (filter)) {
					packageRefs = packageRefs.Where (p => p.PackageIdentity.Id.StartsWith (filter, StringComparison.OrdinalIgnoreCase));
				}

				// Skip and then take
				if (skip != 0) {
					packageRefs = packageRefs.Skip (skip); }
				if (take != 0) {
					packageRefs = packageRefs.Take (take);
				}

				installedPackages.Add (project, packageRefs);
			}

			return installedPackages;
		}

		protected IEnumerable<SourceRepository> PrimarySourceRepositories
		{
			get {
				if (activeSourceRepository != null)
					return new[] { activeSourceRepository };

				return EnabledSourceRepositories;
			}
		}

		protected IEnumerable<IPackageSearchMetadata> GetPackagesFromRemoteSource (
			string searchString,
			bool includePrerelease)
		{
			var searchFilter = new SearchFilter {
				IncludePrerelease = includePrerelease,
				SupportedFrameworks = Enumerable.Empty<string> (),
				IncludeDelisted = false
			};

			var packageFeed = new MultiSourcePackageFeed (PrimarySourceRepositories, logger: null);
			var searchTask = packageFeed.SearchAsync (searchString, searchFilter, ConsoleHost.Token);
			return PackageFeedEnumerator.Enumerate (packageFeed, searchTask, ConsoleHost.Token);
		}

		protected async Task<IPackageSearchMetadata> GetLatestPackageFromRemoteSourceAsync (
			PackageIdentity identity,
			bool includePrerelease)
		{
			var metadataProvider = new MultiSourcePackageMetadataProvider (
				PrimarySourceRepositories,
				optionalLocalRepository: null,
				optionalGlobalLocalRepository: null,
				logger: NuGet.Logging.NullLogger.Instance);
			return await metadataProvider.GetLatestPackageMetadataAsync (identity, includePrerelease, ConsoleHost.Token);
		}
	}
}

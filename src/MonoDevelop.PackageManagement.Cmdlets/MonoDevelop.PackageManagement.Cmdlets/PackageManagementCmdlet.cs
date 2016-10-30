// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ICSharpCode.PackageManagement.Scripting;
using MonoDevelop.Core;
using MonoDevelop.PackageManagement;
using MonoDevelop.Projects;
using NuGet;
using NuGet.PackageManagement;
using NuGet.PackageManagement.UI;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;

using MessageLevel = NuGet.MessageLevel;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	public abstract class PackageManagementCmdlet : PSCmdlet, ITerminatingCmdlet, IPackageScriptSession, IPackageScriptRunner, ILogger, INuGetProjectContext
	{
		IPackageManagementConsoleHost consoleHost;
		ICmdletTerminatingError terminatingError;
		SourceRepository activeSourceRepository;
		bool overwriteAll;
		bool ignoreAll;

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

		public void Log (MessageLevel level, string message, params object[] args)
		{
			string fullMessage = String.Format (message, args);

			switch (level) {
				case MessageLevel.Error:
					WriteError (CreateErrorRecord (fullMessage));
					break;
				case MessageLevel.Warning:
					WriteWarning (fullMessage);
					break;
				case MessageLevel.Debug:
					WriteVerbose (fullMessage);
					break;
				default:
					Host.UI.WriteLine (fullMessage);
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

		public PackageExtractionContext PackageExtractionContext { get; set; }
		public ISourceControlManagerProvider SourceControlManagerProvider { get; private set; }
		public NuGet.ProjectManagement.ExecutionContext ExecutionContext { get; protected set; }
		public XDocument OriginalPackagesConfig { get; set; }
		public NuGetActionType ActionType { get; set; }

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

		protected void PreviewNuGetPackageActions (IEnumerable<NuGetProjectAction> actions)
		{
			if (actions == null || !actions.Any ()) {
				Log (MessageLevel.Info, GettextCatalog.GetString ("No package actions available to be executed."));
			} else {
				foreach (NuGetProjectAction action in actions) {
					Log (MessageLevel.Info, action.NuGetProjectActionType + " " + action.PackageIdentity);
				}
			}
		}

		public void Log (NuGet.ProjectManagement.MessageLevel level, string message, params object [] args)
		{
			Log ((MessageLevel)level, message, args);
		}

		public void ReportError (string message)
		{
		}

		protected FileConflictAction? ConflictAction { get; set; }

		FileConflictAction INuGetProjectContext.ResolveFileConflict (string message)
		{
			if (overwriteAll) {
				return FileConflictAction.OverwriteAll;
			}

			if (ignoreAll) {
				return FileConflictAction.IgnoreAll;
			}

			if (ConflictAction != null && ConflictAction != FileConflictAction.PromptUser) {
				return (FileConflictAction)ConflictAction;
			}

			FileConflictAction result = PackageManagementServices.PackageManagementEvents.OnResolveFileConflict (message);
			switch (result) {
				case FileConflictAction.IgnoreAll:
					ignoreAll = true;
				break;
				case FileConflictAction.OverwriteAll:
					overwriteAll = true;
				break;
			}
			return result;
		}
	}
}

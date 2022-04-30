// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.PackageManagement;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;
using ICSharpCode.PackageManagement.EnvDTE;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.PackageManagement.Scripting;
using NuGet.Common;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.VisualStudio;
using PackageSource = NuGet.Configuration.PackageSource;
using NuGetVersion = NuGet.Versioning.NuGetVersion;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	public abstract class NuGetPowerShellBaseCommand : PSCmdlet, IErrorHandler
	{
		readonly BlockingCollection<Message> blockingCollection = new BlockingCollection<Message> ();

		SourceRepository activeSourceRepository;
		ISourceRepositoryProvider sourceRepositoryProvider;

		internal const string ActivePackageSourceKey = "activePackageSource";
		const string CancellationTokenKey = "CancellationTokenKey";

		public NuGetPowerShellBaseCommand ()
		{
			sourceRepositoryProvider = ServiceLocator.GetInstance<ISourceRepositoryProvider> ();
			SolutionManager = ServiceLocator.GetInstance<IConsoleHostSolutionManager> ();
			DTE = ServiceLocator.GetInstance<DTE> ();
		}

		protected IConsoleHostSolutionManager SolutionManager { get; }

		protected IEnumerable<SourceRepository> PrimarySourceRepositories {
			get {
				return activeSourceRepository != null
					? new[] { activeSourceRepository } : EnabledSourceRepositories;
			}
		}

		/// <summary>
		/// List of all the enabled source repositories
		/// </summary>
		protected IEnumerable<SourceRepository> EnabledSourceRepositories { get; private set; }

		protected IErrorHandler ErrorHandler {
			get { return this; }
		}

		/// <summary>
		/// Determine if needs to log total time elapsed or not
		/// </summary>
		protected virtual bool IsLoggingTimeDisabled { get; }

		/// <summary>
		/// DTE instance for PowerShell Cmdlets
		/// </summary>
		protected DTE DTE { get; }

		protected NuGetProject Project { get; set; }

		protected Project DTEProject { get; set; }

		protected FileConflictAction? ConflictAction { get; set; }

		protected CancellationToken Token {
			get {
				if (Host == null
					|| Host.PrivateData == null) {
					return CancellationToken.None;
				}

				var tokenProp = GetPropertyValueFromHost (CancellationTokenKey);
				if (tokenProp == null) {
					return CancellationToken.None;
				}

				return (CancellationToken)tokenProp;
			}
		}

		protected override sealed void ProcessRecord ()
		{
			var stopWatch = new Stopwatch ();
			stopWatch.Start ();
			try {
				RegisterBlockingCollectionWithHost ();
				ProcessRecordCore ();
			} catch (Exception ex) {
				// unhandled exceptions should be terminating
				ErrorHandler.HandleException (ex, terminating: true);
			} finally {
				UnregisterBlockingCollectionWithHost ();
				UnsubscribeEvents ();
			}

			stopWatch.Stop ();

			// Log total time elapsed except for Tab command
			if (!IsLoggingTimeDisabled) {
				LogCore (MessageLevel.Info, string.Format (CultureInfo.CurrentCulture, "Time Elapsed: {0}", stopWatch.Elapsed));
			}
		}

		void RegisterBlockingCollectionWithHost ()
		{
			ConsoleHostServices.ActiveBlockingCollection = BlockingCollection;
		}

		void UnregisterBlockingCollectionWithHost ()
		{
			ConsoleHostServices.ActiveBlockingCollection = null;
		}

		/// <summary>
		/// Derived classess must implement this method instead of ProcessRecord(), which is sealed by
		/// NuGetPowerShellBaseCommand.
		/// </summary>
		protected abstract void ProcessRecordCore ();

		/// <summary>
		/// Get list of packages from the remote package source. Used for Get-Package -ListAvailable.
		/// </summary>
		/// <param name="searchString">The search string to use for filtering.</param>
		/// <param name="includePrerelease">Whether or not to include prerelease packages in the results.</param>
		/// <param name="handleError">
		/// An action for handling errors during the enumeration of the returned results. The
		/// parameter is the error message. This action is never called by multiple threads at once.
		/// </param>
		/// <returns>The lazy sequence of package search metadata.</returns>
		protected IEnumerable<IPackageSearchMetadata> GetPackagesFromRemoteSource (
			string searchString,
			bool includePrerelease,
			Action<string> handleError)
		{
			var searchFilter = new SearchFilter (includePrerelease: includePrerelease);
			searchFilter.IncludeDelisted = false;
			var packageFeed = new MultiSourcePackageFeed (PrimarySourceRepositories, logger: null);
			var searchTask = packageFeed.SearchAsync (searchString, searchFilter, Token);

			return PackageFeedEnumerator.Enumerate (
				packageFeed,
				searchTask,
				(source, exception) => {
					var message = string.Format (
						CultureInfo.CurrentCulture,
						"The following source failed to search for packages: '{0}'{1}{2}",
						source,
						Environment.NewLine,
						ExceptionUtilities.DisplayMessage (exception));

					handleError (message);
				},
				Token);
		}

		protected async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesFromRemoteSourceAsync (string packageId, bool includePrerelease)
		{
			var metadataProvider = new MultiSourcePackageMetadataProvider (
				PrimarySourceRepositories,
				optionalLocalRepository: null,
				optionalGlobalLocalRepositories: null,
				logger: Common.NullLogger.Instance);

			return await metadataProvider.GetPackageMetadataListAsync (
				packageId,
				includePrerelease,
				includeUnlisted: false,
				cancellationToken: Token);
		}

		protected async Task<IPackageSearchMetadata> GetLatestPackageFromRemoteSourceAsync (IEnumerable<PackageReference> packageReferences, PackageIdentity identity, bool includePrerelease)
		{
			var metadataProvider = new MultiSourcePackageMetadataProvider (
				PrimarySourceRepositories,
				optionalLocalRepository: null,
				optionalGlobalLocalRepositories: null,
				logger: Common.NullLogger.Instance);

			return await metadataProvider.GetLatestPackageMetadataAsync (identity, packageReferences, includePrerelease, Token);
		}

		protected async Task<IEnumerable<string>> GetPackageIdsFromRemoteSourceAsync (string idPrefix, bool includePrerelease)
		{
			var autoCompleteProvider = new MultiSourceAutoCompleteProvider (PrimarySourceRepositories, logger: Common.NullLogger.Instance);
			return await autoCompleteProvider.IdStartsWithAsync (idPrefix, includePrerelease, Token);
		}

		protected async Task<IEnumerable<NuGetVersion>> GetPackageVersionsFromRemoteSourceAsync (string id, string versionPrefix, bool includePrerelease)
		{
			var autoCompleteProvider = new MultiSourceAutoCompleteProvider (PrimarySourceRepositories, logger: Common.NullLogger.Instance);
			var results = await autoCompleteProvider.VersionStartsWithAsync (id, versionPrefix, includePrerelease, Token);
			return results?.OrderByDescending (v => v).ToArray ();
		}

		protected void PreviewNuGetPackageActions (IEnumerable<PackageActionInfo> actions)
		{
			if (actions == null
				|| !actions.Any ()) {
				Log (MessageLevel.Info, "No package actions available to be executed.");
			} else {
				foreach (var action in actions) {
					var version = NuGetVersion.Parse (action.PackageVersion);
					var identity = new PackageIdentity (action.PackageId, version);
					Log (MessageLevel.Info, action.Type + " " + identity);
				}
			}
		}

		protected override void BeginProcessing ()
		{
			IsExecuting = true;
		}

		protected override void EndProcessing ()
		{
			IsExecuting = false;
			UnsubscribeEvents ();
			base.EndProcessing ();
		}

		protected void UnsubscribeEvents ()
		{
		}

		protected SourceValidationResult ValidateSource (string source)
		{
			// If source string is not specified, get the current active package source from the host.
			if (string.IsNullOrEmpty (source)) {
				source = (string)GetPropertyValueFromHost (ActivePackageSourceKey);
			}

			// Look through all available sources (including those disabled) by matching source name and URL (or path).
			var matchingSource = GetMatchingSource (source);
			if (matchingSource != null) {
				return SourceValidationResult.Valid (
					source,
					sourceRepositoryProvider?.CreateRepository (matchingSource));
			}

			// If we really can't find a source string, return an empty validation result.
			if (string.IsNullOrEmpty (source)) {
				return SourceValidationResult.None;
			}

			return CheckSourceValidity (source);
		}

		protected void UpdateActiveSourceRepository (string source)
		{
			var result = ValidateSource (source);
			EnsureValidSource (result);
			UpdateActiveSourceRepository (result.SourceRepository);
		}

		protected void EnsureValidSource (SourceValidationResult result)
		{
			if (result.Validity == SourceValidity.UnknownSource) {
				throw new PackageSourceException (string.Format (
					CultureInfo.CurrentCulture,
					"Source '{0}' not found. Please provide an HTTP or local source.",
					result.Source));
			} else if (result.Validity == SourceValidity.UnknownSourceType) {
				throw new PackageSourceException (string.Format (
					CultureInfo.CurrentCulture,
					"Unsupported type of source '{0}'.Please provide an HTTP or local source.",
					result.Source));
			}
		}

		protected void UpdateActiveSourceRepository (SourceRepository sourceRepository)
		{
			if (sourceRepository != null) {
				activeSourceRepository = sourceRepository;
			}

			EnabledSourceRepositories = sourceRepositoryProvider?.GetRepositories ()
				.Where (r => r.PackageSource.IsEnabled)
				.ToList ();
		}

		/// <summary>
		/// Create a package repository from the source by trying to resolve relative paths.
		/// </summary>
		SourceRepository CreateRepositoryFromSource (string source)
		{
			var packageSource = new PackageSource (source);
			var repository = sourceRepositoryProvider.CreateRepository (packageSource);
			var resource = repository.GetResource<PackageSearchResource> ();

			return repository;
		}

		protected object GetPropertyValueFromHost (string propertyName)
		{
			var privateData = Host.PrivateData;
			var propertyInfo = privateData.Properties [propertyName];
			if (propertyInfo != null) {
				return propertyInfo.Value;
			}
			return null;
		}

		PackageSource GetMatchingSource (string source)
		{
			var packageSources = sourceRepositoryProvider.PackageSourceProvider?.LoadPackageSources ();

			var matchingSource = packageSources?.FirstOrDefault (
				p => StringComparer.OrdinalIgnoreCase.Equals (p.Name, source) ||
					 StringComparer.OrdinalIgnoreCase.Equals (p.Source, source));

			return matchingSource;
		}

		/// <summary>
		/// If a relative local URI is passed, it converts it into an absolute URI.
		/// If the local URI does not exist or it is neither http nor local type, then the source is rejected.
		/// If the URI is not relative then no action is taken.
		/// </summary>
		/// <param name="inputSource">The source string specified by -Source switch.</param>
		/// <returns>The source validation result.</returns>
		SourceValidationResult CheckSourceValidity (string inputSource)
		{
			// Convert file:// to a local path if needed, this noops for other types
			var source = UriUtility.GetLocalPath (inputSource);

			// Convert a relative local URI into an absolute URI
			var packageSource = new PackageSource (source);
			Uri sourceUri;
			if (Uri.TryCreate (source, UriKind.Relative, out sourceUri)) {
				string outputPath;
				bool? exists;
				string errorMessage;
				if (PSPathUtility.TryTranslatePSPath (SessionState, source, out outputPath, out exists, out errorMessage) &&
					exists == true) {
					source = outputPath;
					packageSource = new PackageSource (source);
				} else if (exists == false) {
					return SourceValidationResult.UnknownSource (source);
				}
			} else if (!packageSource.IsHttp) {
				// Throw and unknown source type error if the specified source is neither local nor http
				return SourceValidationResult.UnknownSourceType (source);
			}

			// Check if the source is a valid HTTP URI.
			if (packageSource.IsHttp && packageSource.TrySourceAsUri == null) {
				return SourceValidationResult.UnknownSource (source);
			}

			var sourceRepository = CreateRepositoryFromSource (source);

			return SourceValidationResult.Valid (source, sourceRepository);
		}

		/// <summary>
		/// Check if solution is open. If not, throw terminating error
		/// </summary>
		protected void CheckSolutionState ()
		{
			if (!SolutionManager.IsSolutionOpen) {
				ErrorHandler.ThrowSolutionNotOpenTerminatingError ();
			}
		}

		protected async Task GetNuGetProjectAsync (string projectName = null)
		{
			if (string.IsNullOrEmpty (projectName)) {
				Project = (await SolutionManager.GetDefaultNuGetProjectAsync ());
				if (Project == null) {
					ErrorHandler.WriteProjectNotFoundError ("Default", terminating: true);
				}
			} else {
				Project = (await SolutionManager.GetNuGetProjectAsync (projectName));
				if (Project == null) {
					ErrorHandler.WriteProjectNotFoundError (projectName, terminating: true);
				}
			}
		}

		protected async Task<EnvDTE.Project> GetDefaultProjectAsync ()
		{
			NuGetProject defaultNuGetProject = await SolutionManager.GetDefaultNuGetProjectAsync ();
			// Solution may be open without a project in it. Then defaultNuGetProject is null.
			if (defaultNuGetProject != null) {
				return await SolutionManager.GetEnvDTEProjectAsync (defaultNuGetProject);
			}

			return null;
		}

		/// <summary>
		/// Return all projects in the solution matching the provided names. Wildcards are supported.
		/// This method will automatically generate error records for non-wildcarded project names that
		/// are not found.
		/// </summary>
		/// <param name="projectNames">An array of project names that may or may not include wildcards.</param>
		/// <returns>Projects matching the project name(s) provided.</returns>
		protected async Task<IEnumerable<EnvDTE.Project>> GetProjectsByNameAsync (string[] projectNames)
		{
			var result = new List<EnvDTE.Project> ();
			var allProjects = (await SolutionManager.GetAllNuGetProjectsAsync ())?.ToList ();
			var allValidProjectNames = await GetAllValidProjectNamesAsync (allProjects);
			var matchedNuGetProjects = new HashSet<NuGetProject>();

			foreach (var projectName in projectNames) {
				// if ctrl+c hit, leave immediately
				if (Stopping) {
					break;
				}

				// Treat every name as a wildcard; results in simpler code
				var pattern = new WildcardPattern (projectName, WildcardOptions.IgnoreCase);

				var matches = allValidProjectNames
					.Where (s => pattern.IsMatch (s))
					.ToArray ();

				// We only emit non-terminating error record if a non-wildcarded name was not found.
				// This is consistent with built-in cmdlets that support wildcarded search.
				// A search with a wildcard that returns nothing should not be considered an error.
				if ((matches.Length == 0)
					&& !WildcardPattern.ContainsWildcardCharacters (projectName)) {
					ErrorHandler.WriteProjectNotFoundError (projectName, terminating: false);
				}

				foreach (var match in matches) {
					var matchedProject = GetNuGetProject (allProjects, match);
					if (matchedProject != null && matchedNuGetProjects.Add (matchedProject)) {
						var dteProject = await SolutionManager.GetEnvDTEProjectAsync (matchedProject);
						result.Add (dteProject);
					}
				}
			}

			return result;
		}

		NuGetProject GetNuGetProject (IEnumerable<NuGetProject> projects, string match)
		{
			return projects.FirstOrDefault (project => IsProjectNameMatch (project, match));
		}

		static bool IsProjectNameMatch (NuGetProject project, string name)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (project.GetUniqueName (), name) ||
				StringComparer.OrdinalIgnoreCase.Equals (project.GetName (), name);
		}

		/// <summary>
		/// Return all possibly valid project names in the current solution. This includes all
		/// unique names and safe names.
		/// </summary>
		async Task<IEnumerable<string>> GetAllValidProjectNamesAsync (IEnumerable<NuGetProject> allProjects)
		{
			var safeNames = await Task.WhenAll (allProjects?.Select (p => SolutionManager.GetNuGetProjectSafeNameAsync (p)));
			var uniqueNames = allProjects?.Select (p => p.GetUniqueName ());
			var names = uniqueNames.Concat (safeNames).Distinct ();
			return names;
		}

		/// <summary>
		/// Get the list of installed packages based on Filter, Skip and First parameters. Used for Get-Package.
		/// </summary>
		/// <returns></returns>
		protected static async Task<Dictionary<NuGetProject, IEnumerable<PackageReference>>> GetInstalledPackagesAsync (IEnumerable<NuGetProject> projects,
			string filter,
			int skip,
			int take,
			CancellationToken token)
		{
			var installedPackages = new Dictionary<NuGetProject, IEnumerable<PackageReference>> ();

			foreach (var project in projects) {
				var packageRefs = await project.GetInstalledPackagesAsync (token);
				// Filter the results by string
				if (!string.IsNullOrEmpty (filter)) {
					packageRefs = packageRefs.Where (p => p.PackageIdentity.Id.StartsWith (filter, StringComparison.OrdinalIgnoreCase));
				}

				// Skip and then take
				if (skip != 0) {
					packageRefs = packageRefs.Skip (skip);
				}
				if (take != 0) {
					packageRefs = packageRefs.Take (take);
				}

				installedPackages.Add (project, packageRefs);
			}

			return installedPackages;
		}

		public void HandleError (ErrorRecord errorRecord, bool terminating)
		{
			if (terminating) {
				ThrowTerminatingError (errorRecord);
			} else {
				WriteError (errorRecord);
			}
		}

		public void HandleException (Exception exception, bool terminating,
			string errorId, ErrorCategory category, object target)
		{
			exception = ExceptionUtility.Unwrap (exception);

			var error = new ErrorRecord (exception, errorId, category, target);

			ErrorHandler.HandleError (error, terminating: terminating);
		}

		public void WriteProjectNotFoundError (string projectName, bool terminating)
		{
			var notFoundException =
				new ItemNotFoundException (
					String.Format (
						CultureInfo.CurrentCulture,
						"Project '{0}' is not found.", projectName));

			ErrorHandler.HandleError (
				new ErrorRecord (
					notFoundException,
					NuGetErrorId.ProjectNotFound, // This is your locale-agnostic error id.
					ErrorCategory.ObjectNotFound,
					projectName),
				terminating: terminating);
		}

		public void ThrowSolutionNotOpenTerminatingError ()
		{
			ErrorHandler.HandleException (
				new InvalidOperationException ("The current environment doesn't have a solution open."),
				terminating: true,
				errorId: NuGetErrorId.NoActiveSolution,
				category: ErrorCategory.InvalidOperation);
		}

		public void ThrowNoCompatibleProjectsTerminatingError ()
		{
			ErrorHandler.HandleException (
				new InvalidOperationException ("No compatible project found in the active solution"),
				terminating: true,
				errorId: NuGetErrorId.NoCompatibleProjects,
				category: ErrorCategory.InvalidOperation);
		}

		public bool IsExecuting { get; set; }

		protected void WriteError (string message)
		{
			if (!String.IsNullOrEmpty (message)) {
				WriteError (new Exception (message));
			}
		}

		protected void WriteError (Exception exception)
		{
			ErrorHandler.HandleException (exception, terminating: false);
		}

		protected void WriteLine (string message = null)
		{
			if (Host == null) {
				// Host is null when running unit tests. Simply return in this case
				return;
			}

			if (message == null) {
				Host.UI.WriteLine ();
			} else {
				Host.UI.WriteLine (message);
			}
		}

		/// <summary>
		/// LogCore that write messages to the PowerShell console via PowerShellExecution thread.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="formattedMessage"></param>
		protected virtual void LogCore (MessageLevel level, string formattedMessage)
		{
			switch (level) {
				case MessageLevel.Debug:
					WriteVerbose (formattedMessage);
					break;

				case MessageLevel.Warning:
					WriteWarning (formattedMessage);
					break;

				case MessageLevel.Info:
					WriteLine (formattedMessage);
					break;

				case MessageLevel.Error:
					WriteError (formattedMessage);
					break;
			}
		}

		public void Log (MessageLevel level, string message, params object[] args)
		{
			if (args.Length > 0) {
				message = string.Format (CultureInfo.CurrentCulture, message, args);
			}

			BlockingCollection.Add (new LogMessage (level, message));
		}

		protected void WaitAndLogPackageActions ()
		{
			try {
				while (true) {
					var message = BlockingCollection.Take ();
					if (message is ExecutionCompleteMessage) {
						break;
					}

					var scriptMessage = message as ScriptMessage;
					if (scriptMessage != null) {
						ExecutePSScriptInternal (scriptMessage);
						continue;
					}

					var logMessage = message as LogMessage;
					if (logMessage != null) {
						LogCore (logMessage.Level, logMessage.Content);
						continue;
					}

					//var flushMessage = message as FlushMessage;
					//if (flushMessage != null) {
					//	_flushSemaphore.Release ();
					//}
				}
			} catch (InvalidOperationException ex) {
				LogCore (MessageLevel.Error, ExceptionUtilities.DisplayMessage (ex));
			}
		}

		void ExecutePSScriptInternal (ScriptMessage message)
		{
			try {
				var request = new ScriptExecutionRequest (
					message.ScriptPath,
					message.InstallPath,
					message.Identity,
					message.Project);

				var psVariable = SessionState.PSVariable;

				// set temp variables to pass to the script
				psVariable.Set ("__rootPath", request.InstallPath);
				psVariable.Set ("__toolsPath", request.ToolsPath);
				psVariable.Set ("__package", request.ScriptPackage);
				psVariable.Set ("__project", request.Project);

				if (request.ScriptPath != null) {
					string command = "& " + PathUtility.EscapePSPath (request.ScriptPath) + " $__rootPath $__toolsPath $__package $__project";
					LogCore (MessageLevel.Info, String.Format (CultureInfo.CurrentCulture, "Executing script file '{0}'", request.ScriptPath));

					InvokeCommand.InvokeScript (command, false, PipelineResultTypes.Error, null, null);
				}

				// clear temp variables
				SessionState.PSVariable.Remove ("__rootPath");
				SessionState.PSVariable.Remove ("__toolsPath");
				SessionState.PSVariable.Remove ("__package");
				SessionState.PSVariable.Remove ("__project");
			} catch (Exception ex) {
				message.Exception = ex;
			} finally {
				message.EndSemaphore.Release ();
			}
		}

		protected BlockingCollection<Message> BlockingCollection => blockingCollection;
	}
}

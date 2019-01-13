// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.VisualStudio;
using NuGet.Versioning;
using NuGet.Configuration;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	public abstract class NuGetPowerShellBaseCommand : PSCmdlet, IErrorHandler
	{
		SourceRepository activeSourceRepository;

		public NuGetPowerShellBaseCommand ()
		{
			string rootDirectory = null;
			var settings = Settings.LoadDefaultSettings (rootDirectory, null, null);
			var packageSourceProvider = new PackageSourceProvider (settings);
			var provider = new SourceRepositoryProvider (packageSourceProvider, Repository.Provider.GetVisualStudio ());

			EnabledSourceRepositories = provider.GetRepositories ();
		}

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

		protected NuGetProject Project { get; set; }

		protected CancellationToken Token {
			get {
				if (Host == null
					|| Host.PrivateData == null) {
					return CancellationToken.None;
				}

				//var tokenProp = GetPropertyValueFromHost (CancellationTokenKey);
				//if (tokenProp == null) {
					return CancellationToken.None;
				//}

				//return (CancellationToken)tokenProp;
			}
		}

		protected override sealed void ProcessRecord ()
		{
			try {
				ProcessRecordCore ();
			} catch (Exception ex) {
				// unhandled exceptions should be terminating
				ErrorHandler.HandleException (ex, terminating: true);
			} finally {
				UnsubscribeEvents ();
			}
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

		protected async Task<IPackageSearchMetadata> GetLatestPackageFromRemoteSourceAsync (PackageIdentity identity, bool includePrerelease)
		{
			var metadataProvider = new MultiSourcePackageMetadataProvider (
				PrimarySourceRepositories,
				optionalLocalRepository: null,
				optionalGlobalLocalRepositories: null,
				logger: Common.NullLogger.Instance);

			return await metadataProvider.GetLatestPackageMetadataAsync (identity, Project, includePrerelease, Token);
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

		protected void UpdateActiveSourceRepository (string source)
		{
			//var result = ValidateSource (source);
			//EnsureValidSource (result);
			//UpdateActiveSourceRepository (result.SourceRepository);
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
	}
}

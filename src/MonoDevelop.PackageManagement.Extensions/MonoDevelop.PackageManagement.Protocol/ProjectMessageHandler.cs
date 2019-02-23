//
// ProjectMessageHandler.cs
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;
using MonoDevelop.Projects.MSBuild;
using Newtonsoft.Json.Linq;
using NuGet.PackageManagement;
using NuGet.Packaging;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.Protocol
{
	class ProjectMessageHandler
	{
		CancellationToken Token {
			get { return PackageManagementExtendedServices.ConsoleHost.Token; }
		}

		[JsonRpcMethod (Methods.ProjectInstalledPackagesName)]
		public ProjectPackagesList OnGetInstalledPackages (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectParams> ();
				var project = FindProject (message.FileName);
				var packages = GetInstalledPackages (project).WaitAndGetResult ();
				return new ProjectPackagesList {
					Packages = CreatePackageInformation (packages).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetInstalledPackages error", ex);
				throw;
			}
		}

		async Task<IEnumerable<PackageReference>> GetInstalledPackages (DotNetProject project)
		{
			var solutionManager = project.GetSolutionManager ();
			var nugetProject =  project.CreateNuGetProject (solutionManager);

			return await Task.Run (() => nugetProject.GetInstalledPackagesAsync (Token)).ConfigureAwait (false);
		}

		IEnumerable<PackageReferenceInfo> CreatePackageInformation (IEnumerable<PackageReference> packages)
		{
			return packages.Select (package => CreatePackageInformation (package));
		}

		PackageReferenceInfo CreatePackageInformation (PackageReference package)
		{
			return new PackageReferenceInfo {
				Id = package.PackageIdentity.Id,
				Version = package.PackageIdentity.Version.ToNormalizedString (),
				VersionRange = package.AllowedVersions?.ToNormalizedString (),
				TargetFramework = package.TargetFramework.ToString (),
				IsDevelopmentDependency = package.IsDevelopmentDependency,
				IsUserInstalled = package.IsUserInstalled,
				RequireReinstallation = package.RequireReinstallation
			};
		}

		DotNetProject FindProject (string fileName)
		{
			var matchedProject = PackageManagementServices
				.ProjectService
				.GetOpenProjects ()
				.FirstOrDefault (project => project.FileName == fileName);

			return matchedProject.DotNetProject;
		}

		IEnumerable<DotNetProject> FindProjects (IEnumerable<string> fileNames)
		{
			var allProjects = PackageManagementServices
				.ProjectService
				.GetOpenProjects ();

			var matchedProjects = new List<DotNetProject> ();

			foreach (string fileName in fileNames) {
				var matchedProject = allProjects.FirstOrDefault (project => project.FileName == fileName);
				if (matchedProject == null) {
					throw new ArgumentException ("Could not find project '{0}'", fileName);
				}
				matchedProjects.Add (matchedProject.DotNetProject);
			}

			return matchedProjects;
		}

		[JsonRpcMethod (Methods.ProjectPreviewUninstallPackage)]
		public PackageActionList OnPreviewUninstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<UninstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				var handler = new UninstallPackageMessageHandler (project, message);
				var actions = handler.PreviewUninstallPackageAsync (Token).WaitAndGetResult ();
				return new PackageActionList {
					Actions = CreatePackageActionInformation (actions).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnPreviewUninstallPackage error", ex);
				throw;
			}
		}

		IEnumerable<PackageActionInfo> CreatePackageActionInformation (IEnumerable<NuGetProjectAction> actions)
		{
			return actions.Select (action => CreatePackageActionInformation (action));
		}

		PackageActionInfo CreatePackageActionInformation (NuGetProjectAction package)
		{
			return new PackageActionInfo {
				PackageId = package.PackageIdentity.Id,
				PackageVersion = package.PackageIdentity.Version.ToNormalizedString (),
				Type = package.NuGetProjectActionType.ToString ()
			};
		}

		[JsonRpcMethod (Methods.ProjectUninstallPackage)]
		public void OnUninstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<UninstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				var handler = new UninstallPackageMessageHandler (project, message);
				handler.UninstallPackageAsync (Token).WaitAndGetResult ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnUninstallPackage error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectPreviewInstallPackage)]
		public InstallPackageActionList OnPreviewInstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<InstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				var handler = new InstallPackageMessageHandler (project, message);
				var actions = handler.PreviewInstallPackageAsync (Token).WaitAndGetResult ();
				return new InstallPackageActionList {
					IsPackageAlreadyInstalled = handler.IsPackageAlreadyInstalled,
					Actions = CreatePackageActionInformation (actions).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnPreviewInstallPackage error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectInstallPackage)]
		public InstallPackageResult OnInstallPackage (JToken arg)
		{
			try {
				var message = arg.ToObject<InstallPackageParams> ();
				var project = FindProject (message.ProjectFileName);
				var handler = new InstallPackageMessageHandler (project, message);
				handler.InstallPackageAsync (Token).WaitAndGetResult ();
				return new InstallPackageResult {
					IsPackageAlreadyInstalled = handler.IsPackageAlreadyInstalled
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnInstallPackage error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectPreviewUpdatePackage)]
		public UpdatePackageActionList OnPreviewUpdatePackage (JToken arg)
		{
			try {
				var message = arg.ToObject<UpdatePackageParams> ();
				var projects = FindProjects (message.ProjectFileNames);
				var handler = new UpdatePackageMessageHandler (projects, message);
				var actions = handler.PreviewUpdatePackageAsync (Token).WaitAndGetResult ();
				return new UpdatePackageActionList {
					IsPackageInstalled = handler.IsPackageInstalled,
					Actions = CreatePackageActionInformation (actions).ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnPreviewInstallPackage error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectUpdatePackage)]
		public UpdatePackageResult OnUpdatePackage (JToken arg)
		{
			try {
				var message = arg.ToObject<UpdatePackageParams> ();
				var projects = FindProjects (message.ProjectFileNames);
				var handler = new UpdatePackageMessageHandler (projects, message);
				handler.UpdatePackageAsync (Token).WaitAndGetResult ();
				return new UpdatePackageResult {
					IsPackageInstalled = handler.IsPackageInstalled
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnUpdatePackage error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectUpdateAllPackages)]
		public void OnUpdateAllPackages (JToken arg)
		{
			try {
				var message = arg.ToObject<UpdatePackageParams> ();
				var projects = FindProjects (message.ProjectFileNames);
				var handler = new UpdatePackageMessageHandler (projects, message);
				handler.UpdateAllPackagesAsync (Token).WaitAndGetResult ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnUpdateAllPackages error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectPropertyValueName)]
		public PropertyValueInfo OnGetProjectPropertyValue (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectPropertyParams> ();
				var project = FindProject (message.ProjectFileName);

				string propertyName = MapPropertyName (message.PropertyName);

				IMetadataProperty property = project.ProjectProperties.GetProperty (propertyName);
				if (property != null) {
					return new PropertyValueInfo {
						PropertyValue = property.Value
					};
				}

				IMSBuildPropertyEvaluated evaluatedProperty = project
					.MSBuildProject?
					.EvaluatedProperties?
					.GetProperty (propertyName);
				if (evaluatedProperty != null) {
					return new PropertyValueInfo {
						PropertyValue = evaluatedProperty.Value
					};
				}

				return new PropertyValueInfo ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetProjectPropertyValue error", ex);
				throw;
			}
		}

		/// <summary>
		/// EnvDTE.Project.Properties supports OutputFileName which maps to TargetFileName.
		/// This is the assembly filename without any path information.
		/// </summary>
		static string MapPropertyName (string propertyName)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals ("OutputFileName", propertyName)) {
				return "TargetFileName";
			}
			return propertyName;
		}

		[JsonRpcMethod (Methods.ProjectConfigurationPropertyValueName)]
		public PropertyValueInfo OnGetProjectConfigurationPropertyValue (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectConfigurationPropertyParams> ();
				var project = FindProject (message.ProjectFileName);

				var configuration = project.GetConfiguration (IdeApp.Workspace.ActiveConfiguration) as ProjectConfiguration;
				if (configuration == null) {
					return new PropertyValueInfo ();
				}

				IMetadataProperty property = configuration.Properties.GetProperty (message.PropertyName);
				if (property != null) {
					return new PropertyValueInfo {
						PropertyValue = GetPropertyValue (property)
					};
				}

				return new PropertyValueInfo ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetProjectConfigurationPropertyValue error", ex);
				throw;
			}
		}

		static string GetPropertyValue (IMetadataProperty property)
		{
			if (IsPathProperty (property.Name)) {
				return GetPathValue (property.Value);
			}

			return property.Value;
		}

		static string GetPathValue (string value)
		{
			if (value == null) {
				return value;
			}

			if (Path.DirectorySeparatorChar == '\\') {
				return value;
			}

			return value.Replace ('\\', '/');
		}

		static bool IsPathProperty (string name)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals ("OutputPath", name)) {
				return true;
			} else if (StringComparer.OrdinalIgnoreCase.Equals ("IntermediateOutputPath", name)) {
				return true;
			}
			return false;
		}

		[JsonRpcMethod (Methods.ProjectItemsName)]
		public ProjectItemInformationList OnGetProjectItems (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectItemInformationParams> ();
				var project = FindProject (message.ProjectFileName);

				var items = new ProjectItemsInsideProject (project);

				return new ProjectItemInformationList {
					Items = items.GetProjectItems ().ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetProjectItems error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectAnalyzerItemsName)]
		public AnalyzerInformationList OnGetProjectAnalyzerItems (JToken arg)
		{
			try {
				var message = arg.ToObject<ProjectItemInformationParams> ();
				var project = FindProject (message.ProjectFileName);

				var fileNames = project.GetAnalyzerFilesAsync (IdeApp.Workspace.ActiveConfiguration)
					.WaitAndGetResult ();

				return new AnalyzerInformationList {
					FileNames = fileNames
						.Select (fileName => fileName.ToString ())
						.ToArray ()
				};
			} catch (Exception ex) {
				LoggingService.LogError ("OnGetProjectAnalyzerItems error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectAddAnalyzerName)]
		public void OnAddAnalyzer (JToken arg)
		{
			try {
				var message = arg.ToObject<AnalzyerInformationParams> ();
				var project = FindProject (message.ProjectFileName);

				var analyzerItem = new ProjectFile (message.AnalyzerFileName, "Analyzer");
				Runtime.RunInMainThread (async () => {
					project.Items.Add (analyzerItem);
					await project.SaveAsync (new ProgressMonitor ());
				})
				.WaitAndGetResult ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnAddAnalyzer error", ex);
				throw;
			}
		}

		[JsonRpcMethod (Methods.ProjectRemoveAnalyzerName)]
		public void OnRemoveAnalyzer (JToken arg)
		{
			try {
				var message = arg.ToObject<AnalzyerInformationParams> ();
				var project = FindProject (message.ProjectFileName);

				var analyzerItem = project.GetProjectFile (message.AnalyzerFileName);
				if (analyzerItem == null) {
					return;
				}

				Runtime.RunInMainThread (async () => {
					project.Items.Remove (analyzerItem);
					await project.SaveAsync (new ProgressMonitor ());
				})
				.WaitAndGetResult ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnRemoveAnalyzer error", ex);
				throw;
			}
		}
	}
}

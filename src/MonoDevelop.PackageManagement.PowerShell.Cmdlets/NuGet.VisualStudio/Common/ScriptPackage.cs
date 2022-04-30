// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NuGet.PackageManagement.VisualStudio
{
	// this scriptPackage is a package inferface for script executor
	// it provides an IPackage like interface to make sure all install.ps scripts which depend on IPackage keep working
	public class ScriptPackage : IScriptPackage
	{
		string id;
		string version;
		string installPath;
		private IList<IPackageAssemblyReference> assemblyReferences;
		private IEnumerable<IScriptPackageFile> files;

		public ScriptPackage (string id, string version, string installPath)
		{
			this.id = id;
			this.version = version;
			this.installPath = installPath;
		}

		public string Id {
			get { return id; }
		}

		public string Version {
			get { return version; }
		}

		public IEnumerable<IPackageAssemblyReference> AssemblyReferences {
			get {
				if (assemblyReferences == null) {
					assemblyReferences = GetAssemblyReferencesCore ().ToList ();
				}

				return assemblyReferences;
			}
		}

		public IEnumerable<IScriptPackageFile> GetFiles ()
		{
			if (files == null) {
				var result = new List<ScriptPackageFile> ();
				using (var reader = GetPackageReader ()) {
					Debug.Assert (reader != null);

					if (reader != null) {
						result.AddRange (GetPackageFiles (reader.GetLibItems ()));
						result.AddRange (GetPackageFiles (reader.GetToolItems ()));
						result.AddRange (GetPackageFiles (reader.GetContentItems ()));
						result.AddRange (GetPackageFiles (reader.GetBuildItems ()));
						result.AddRange (reader.GetFiles ()
							.Where (path => IsUnknownPath (path))
							.Select (p => new ScriptPackageFile (p, NuGetFramework.AnyFramework)));
					}
				}

				files = result;
			}

			return files;
		}

		IEnumerable<IPackageAssemblyReference> GetAssemblyReferencesCore ()
		{
			var result = new List<PackageAssemblyReference> ();
			if (Directory.Exists (installPath)) {
				var nupkg = new FileInfo (
					Path.Combine (installPath, Id + "." + Version + PackagingCoreConstants.NupkgExtension));
				if (nupkg.Exists) {
					var referenceItems = GetReferenceItems (nupkg);
					var files = NuGetFrameworkUtility.GetNearest<FrameworkSpecificGroup> (referenceItems,
																						 NuGetFramework.AnyFramework);
					if (files != null) {
						result = files.Items.Select (file => new PackageAssemblyReference (file)).ToList ();
					}
				}
			}

			return result;
		}

		private static IEnumerable<FrameworkSpecificGroup> GetReferenceItems (FileInfo nupkg)
		{
			using (var reader = new PackageArchiveReader (nupkg.OpenRead ())) {
				return reader.GetReferenceItems ();
			}
		}

		private PackageReaderBase GetPackageReader ()
		{
			if (Directory.Exists (installPath)) {
				var nupkg = new FileInfo (
						Path.Combine (installPath, Id + "." + Version + PackagingCoreConstants.NupkgExtension));
				if (nupkg.Exists) {
					return new PackageArchiveReader (nupkg.OpenRead ());
				}
			}

			return null;
		}

		private IEnumerable<ScriptPackageFile> GetPackageFiles (IEnumerable<FrameworkSpecificGroup> frameworkGroups)
		{
			var result = new List<ScriptPackageFile> ();

			foreach (var frameworkGroup in frameworkGroups) {
				var framework = frameworkGroup.TargetFramework;
				result.AddRange (frameworkGroup.Items.Select (item => new ScriptPackageFile (item, framework)));
			}

			return result;
		}

		private bool IsUnknownPath (string path)
		{
			return PackageHelper.IsPackageFile (path, PackageSaveMode.Defaultv2)
				   && !path.StartsWith ("lib", StringComparison.OrdinalIgnoreCase)
				   && !path.StartsWith ("tools", StringComparison.OrdinalIgnoreCase)
				   && !path.StartsWith ("content", StringComparison.OrdinalIgnoreCase)
				   && !path.StartsWith ("build", StringComparison.OrdinalIgnoreCase);
		}
	}
}
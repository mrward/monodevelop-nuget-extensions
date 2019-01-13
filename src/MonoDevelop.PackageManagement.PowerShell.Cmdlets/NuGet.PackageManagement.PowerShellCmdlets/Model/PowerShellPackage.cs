// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using NuGet.Versioning;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	/// <summary>
	/// Represent the view of packages by Id and Versions
	/// </summary>
	public class PowerShellPackage
	{
		public string Id { get; set; }

		public IEnumerable<NuGetVersion> Versions {
			get {
				var versions = Task.Run (async delegate {
					var result = (await AsyncLazyVersions.GetValueAsync ()) ?? Enumerable.Empty<NuGetVersion> ();

					if (result.Any ()) {
						if (AllVersions) {
							return result;
						} else {
							// result has at least 1 element
							return result.Take (1);
						}
					}

					return null;
				});
				versions.Wait ();
				return versions.Result;
			}
		}

		public AsyncLazy<IEnumerable<NuGetVersion>> AsyncLazyVersions { get; set; }

		public NuGet.SemanticVersion Version {
			get {
				var nVersion = Versions.FirstOrDefault ();

				if (nVersion != null) {
					NuGet.SemanticVersion sVersion;
					NuGet.SemanticVersion.TryParse (nVersion.ToNormalizedString (), out sVersion);
					return sVersion;
				}

				return null;
			}
		}

		public bool AllVersions { get; set; }

		public string LicenseUrl { get; set; }
	}
}
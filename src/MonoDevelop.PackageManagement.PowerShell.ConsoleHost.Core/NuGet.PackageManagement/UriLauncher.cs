// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NuGet.Configuration;
using MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core;

namespace NuGet.PackageManagement
{
	/// <summary>
	/// Based on NuGet's UriHelper but opening an url works by not using Process.Start but the IDE.
	/// </summary>
	public static class UriLauncher
	{
		/// <summary>
		/// Open external link
		/// </summary>
		/// <param name="url"></param>
		public static void OpenExternalLink (Uri url)
		{
			if (url == null
				|| !url.IsAbsoluteUri) {
				return;
			}

			// mitigate security risk
			if (url.IsFile
				|| url.IsLoopback
				|| url.IsUnc) {
				return;
			}

			if (IsHttpUrl (url)) {
				// REVIEW: Will this allow a package author to execute arbitrary program on user's machine?
				// We have limited the url to be HTTP only, but is it sufficient?
				ConsoleHostServices.DTE.ItemOperations.Navigate (url.AbsoluteUri);
			}
		}

		static bool IsHttpUrl (Uri uri)
		{
			return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
		}
	}
}
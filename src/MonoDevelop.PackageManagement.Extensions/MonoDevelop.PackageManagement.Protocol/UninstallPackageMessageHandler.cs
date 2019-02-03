//
// UninstallPackageMessageHandler.cs
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using MonoDevelop.Projects;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;

namespace MonoDevelop.PackageManagement.Protocol
{
	class UninstallPackageMessageHandler
	{
		DotNetProject project;
		UninstallPackageParams message;
		IMonoDevelopSolutionManager solutionManager;
		NuGetProject nugetProject;
		MonoDevelopNuGetPackageManager packageManager;
		UninstallationContext uninstallationContext;
		INuGetProjectContext projectContext;

		public UninstallPackageMessageHandler (DotNetProject project, UninstallPackageParams message)
		{
			this.project = project;
			this.message = message;
		}

		public async Task<IEnumerable<NuGetProjectAction>> PreviewUninstallPackageAsync (
			CancellationToken token)
		{
			solutionManager = project.GetSolutionManager ();
			nugetProject = project.CreateNuGetProject (solutionManager);

			packageManager = new MonoDevelopNuGetPackageManager (solutionManager);
			uninstallationContext = new UninstallationContext (message.RemoveDependencies, message.Force);
			projectContext = new ConsoleHostNuGetProjectContext (solutionManager.Settings);

			return await packageManager.PreviewUninstallPackageAsync (
				nugetProject,
				message.PackageId,
				uninstallationContext,
				projectContext,
				token
			).ConfigureAwait (false);
		}

		public async Task UninstallPackageAsync (CancellationToken token)
		{
			var actions = await PreviewUninstallPackageAsync (token);

			await packageManager.ExecuteNuGetProjectActionsAsync (
				nugetProject,
				actions,
				projectContext,
				NullSourceCacheContext.Instance,
				token
			).ConfigureAwait (false);
		}
	}
}

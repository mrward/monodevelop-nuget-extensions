//
// Methods.cs
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

namespace MonoDevelop.PackageManagement.PowerShell.Protocol
{
	public static class Methods
	{
		public const string InvokeName = "pshost/Invoke";
		public const string ActiveSourceName = "pshost/ActiveSource";
		public const string PackageSourcesChangedName = "pshost/PackageSourcesChanged";
		public const string MaxVisibleColumnsChangedName = "pshost/MaxVisibleColumnsChanged";

		public const string SolutionLoadedName = "pshost/SolutionLoaded";
		public const string SolutionUnloadedName = "pshost/SolutionUnloaded";
		public const string DefaultProjectChangedName = "pshost/DefaultProjectChanged";
		public const string StopCommandName = "pshost/StopCommand";

		public const string RunScript = "pshost/RunScript";
		public const string RunInitScript = "pshost/RunInitScript";

		public const string TabExpansionName = "pshost/TabExpansion";

		public const string LogName = "ps/Log";
		public const string ClearHostName = "ps/ClearHost";
		public const string ShowConsoleName = "ps/ShowConsole";

		public const string PromptForInputName = "ps/PromptForInput";

		public const string ItemOperationsNavigateName = "itemOperations/Navigate";
		public const string ItemOperationsOpenFileName = "itemOperations/OpenFile";

		public const string SolutionProjects = "solution/Projects";
		public const string StartupProjectsName = "solution/StartupProjects";
		public const string BuildSolutionName = "solution/Build";

		public const string ProjectPropertyValueName = "project/PropertyValue";
		public const string ProjectConfigurationPropertyValueName = "project/ConfigurationPropertyValue";
		public const string ProjectItemsName = "project/Items";

		public const string ProjectInstalledPackagesName = "project/InstalledPackages";

		public const string ProjectPreviewUninstallPackage = "project/PreviewUninstallPackage";
		public const string ProjectUninstallPackage = "project/UninstallPackage";

		public const string ProjectPreviewInstallPackage = "project/PreviewInstallPackage";
		public const string ProjectInstallPackage = "project/InstallPackage";

		public const string ProjectPreviewUpdatePackage = "project/PreviewUpdatePackage";
		public const string ProjectUpdatePackage = "project/UpdatePackage";
		public const string ProjectUpdateAllPackages = "project/UpdateAllPackages";
	}
}

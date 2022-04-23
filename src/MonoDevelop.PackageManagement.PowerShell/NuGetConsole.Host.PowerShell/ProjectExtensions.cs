// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Management.Automation;

namespace NuGetConsole.Host.PowerShell.Implementation
{
	public static class ProjectExtensions
	{
		/// <summary>
		/// This method is used for the ProjectName CodeProperty in Types.ps1xml
		/// </summary>
		public static string GetCustomUniqueName (PSObject psObject)
		{
			var project = (EnvDTE.Project)psObject.BaseObject;
			return project.UniqueName;

			// TODO - Implement EnvDTEProjectInfoUtility
			//return NuGetUIThreadHelper.JoinableTaskFactory.Run (async delegate
			//{
			//	await NuGetUIThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync ();

			//	return await EnvDTEProjectInfoUtility.GetCustomUniqueNameAsync (
			//		(EnvDTE.Project)psObject.BaseObject);
			//});
		}
	}
}

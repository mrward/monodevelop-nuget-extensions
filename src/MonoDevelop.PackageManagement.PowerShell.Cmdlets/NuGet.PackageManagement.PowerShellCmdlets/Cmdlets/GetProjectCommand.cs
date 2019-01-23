// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Management.Automation;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	/// <summary>
	/// This cmdlet returns the list of project names in the current solution,
	/// which is also used for tab expansion.
	/// </summary>
	[Cmdlet (VerbsCommon.Get, "Project", DefaultParameterSetName = ParameterSetByName)]
	[OutputType (typeof (EnvDTE.Project))]
	public class GetProjectCommand : NuGetPowerShellBaseCommand
	{
		private const string ParameterSetByName = "ByName";
		private const string ParameterSetAllProjects = "AllProjects";

		[Parameter (Mandatory = false, Position = 0, ParameterSetName = ParameterSetByName, ValueFromPipelineByPropertyName = true)]
		[ValidateNotNullOrEmpty]
		public string[] Name { get; set; }

		[Parameter (Mandatory = true, ParameterSetName = ParameterSetAllProjects)]
		public SwitchParameter All { get; set; }

		void Preprocess ()
		{
			CheckSolutionState ();
		}

		protected override void ProcessRecordCore ()
		{
			Preprocess ();

			if (All.IsPresent) {
				var projects = SolutionManager.GetAllProjectsAsync ().Result;
				WriteObject (projects, enumerateCollection: true);
			} else {
				// No name specified; return default project (if not null)
				if (Name == null) {
					var defaultProject = GetDefaultProjectAsync ().Result;
					if (defaultProject != null) {
						WriteObject (defaultProject);
					}
				} else {
					// get all projects matching name(s) - handles wildcards
					var projects = GetProjectsByNameAsync (Name).Result;
					WriteObject (projects, enumerateCollection: true);
				}
			}
		}
	}
}
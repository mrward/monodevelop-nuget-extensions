// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.PackageManagement.Scripting;
using NuGet;

namespace ICSharpCode.PackageManagement
{
	public class UpdatePackageInAllProjects : UpdatePackageActions2
	{
		IPackageManagementSolution2 solution;
		IPackageRepository sourceRepository;
		List<IPackageManagementProject2> projects;
		PackageReference packageReference;

		public UpdatePackageInAllProjects (
			PackageReference packageReference,
			IPackageManagementSolution2 solution,
			IPackageRepository sourceRepository)
		{
			this.packageReference = packageReference;
			this.solution = solution;
			this.sourceRepository = sourceRepository;
		}

		public override IEnumerable<UpdatePackageAction2> CreateActions ()
		{
			GetProjects ();
			foreach (IPackageManagementProject2 project in projects) {
				yield return CreateUpdatePackageAction (project);
			}
		}

		void GetProjects ()
		{
			projects = new List<IPackageManagementProject2> ();
			projects.AddRange (solution.GetProjects (sourceRepository));
		}

		UpdatePackageAction2 CreateUpdatePackageAction (IPackageManagementProject2 project)
		{
			UpdatePackageAction2 action = CreateDefaultUpdatePackageAction (project);
			action.PackageId = packageReference.Id;
			action.PackageVersion = packageReference.Version;
			return action;
		}
	}
}

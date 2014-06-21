// 
// UpdateAllPackagesInSolution.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2013 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace ICSharpCode.PackageManagement
{
	public class UpdateAllPackagesInSolution2 : UpdatePackageActions2
	{
		IPackageManagementSolution2 solution;
		IPackageRepository sourceRepository;
		List<IPackageManagementProject2> projects;

		public UpdateAllPackagesInSolution2 (
			IPackageManagementSolution2 solution,
			IPackageRepository sourceRepository)
		{
			this.solution = solution;
			this.sourceRepository = sourceRepository;
		}

		public override IEnumerable<UpdatePackageAction2> CreateActions ()
		{
			GetProjects ();
			foreach (IPackage package in GetPackages()) {
				foreach (IPackageManagementProject2 project in projects) {
					yield return CreateAction (project, package);
				}
			}
		}

		void GetProjects ()
		{
			projects = new List<IPackageManagementProject2> ();
			projects.AddRange (solution.GetProjects (sourceRepository));
		}

		IEnumerable<IPackage> GetPackages ()
		{
			return solution.GetPackagesInReverseDependencyOrder ();
		}

		UpdatePackageAction2 CreateAction (IPackageManagementProject2 project, IPackage package)
		{
			UpdatePackageAction2 action = CreateDefaultUpdatePackageAction (project);
			action.PackageId = package.Id;
			return action;
		}
	}
}
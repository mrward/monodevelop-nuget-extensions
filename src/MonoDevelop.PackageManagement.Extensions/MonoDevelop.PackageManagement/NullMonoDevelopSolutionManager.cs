//
// NullMonoDevelopSolutionManager.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
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
//

using System;
using System.Collections.Generic;
using MonoDevelop.Core;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Protocol.Core.Types;

namespace MonoDevelop.PackageManagement
{
	class NullMonoDevelopSolutionManager : IMonoDevelopSolutionManager
	{
		public NullMonoDevelopSolutionManager ()
		{
			ReloadSettings ();
		}

		public NuGet.ProjectManagement.NuGetProject DefaultNuGetProject {
			get {
				throw new NotImplementedException ();
			}
		}

		public string DefaultNuGetProjectName {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public bool IsSolutionAvailable {
			get { return false; }
		}

		public bool IsSolutionOpen {
			get { return false; }
		}

		public NuGet.ProjectManagement.INuGetProjectContext NuGetProjectContext {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public ISettings Settings { get; private set; }

		public string SolutionDirectory {
			get {
				throw new NotImplementedException ();
			}
		}

		#pragma warning disable 67
		public event EventHandler<ActionsExecutedEventArgs> ActionsExecuted;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectAdded;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRemoved;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRenamed;
		public event EventHandler SolutionClosed;
		public event EventHandler SolutionClosing;
		public event EventHandler SolutionOpened;
		public event EventHandler SolutionOpening;
		#pragma warning restore 67

		public ISourceRepositoryProvider CreateSourceRepositoryProvider ()
		{
			return SourceRepositoryProviderFactory.CreateSourceRepositoryProvider (Settings);
		}

		public NuGet.ProjectManagement.NuGetProject GetNuGetProject (string nuGetProjectSafeName)
		{
			throw new NotImplementedException ();
		}

		public NuGet.ProjectManagement.NuGetProject GetNuGetProject (IDotNetProject project)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<NuGet.ProjectManagement.NuGetProject> GetNuGetProjects ()
		{
			throw new NotImplementedException ();
		}

		public string GetNuGetProjectSafeName (NuGet.ProjectManagement.NuGetProject nuGetProject)
		{
			throw new NotImplementedException ();
		}

		public void OnActionsExecuted (IEnumerable<ResolvedAction> actions)
		{
			throw new NotImplementedException ();
		}

		public void ReloadSettings ()
		{
			try {
				LoadSettings ();
			} catch (Exception ex) {
				LoggingService.LogError ("Failed to reload settings.", ex);
			}
		}

		void LoadSettings ()
		{
			Settings = SettingsLoader.LoadDefaultSettings (null, reportError: true);
		}
	}
}

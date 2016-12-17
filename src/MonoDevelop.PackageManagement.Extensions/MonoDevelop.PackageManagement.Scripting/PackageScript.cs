// 
// PackageScript.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using ICSharpCode.PackageManagement.EnvDTE;
using MonoDevelop.PackageManagement;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging.Core;

namespace ICSharpCode.PackageManagement.Scripting
{
	internal class PackageScript : IPackageScript
	{
		public PackageScript (
			string scriptPath,
			string installPath,
			PackageIdentity identity,
			IDotNetProject project)
		{
			ScriptPath = scriptPath;
			InstallPath = installPath;
			Identity = identity;
			Project = project;

			ToolsPath = Path.GetDirectoryName (ScriptPath);
			ScriptPackage = new ScriptPackage (Identity.Id, Identity.Version.ToString (), InstallPath);
		}
		
		public PackageIdentity Identity { get; private set; }
		public IDotNetProject Project { get; private set; }
		public string ScriptPath { get; private set; }
		public string InstallPath { get; private set; }
		public string ToolsPath { get; private set; }
		public ScriptPackage ScriptPackage { get; private set; }
		IPackageScriptSession Session { get; set; }
		
		public void Run (IPackageScriptSession session)
		{
			Session = session;
			Run ();
		}
		
		void Run()
		{
			AddSessionVariables ();
			RunScript ();
			RemoveSessionVariables ();
		}

		void AddSessionVariables ()
		{
			Session.AddVariable ("__rootPath", InstallPath);
			Session.AddVariable ("__toolsPath", ToolsPath);
			Session.AddVariable ("__package", ScriptPackage);
			Session.AddVariable ("__project", GetProject ());
		}
		
		Project GetProject ()
		{
			if (Project != null) {
				return new EnvDTE.Project (Project.DotNetProject);
			}
			return null;
		}
		
		void RunScript()
		{
			string script = GetScript ();
			Session.InvokeScript (script);
		}
		
		string GetScript ()
		{
			return String.Format (
				"& '{0}' $__rootPath $__toolsPath $__package $__project",
				ScriptPath);
		}
		
		void RemoveSessionVariables ()
		{
			Session.RemoveVariable ("__rootPath");
			Session.RemoveVariable ("__toolsPath");
			Session.RemoveVariable ("__package");
			Session.RemoveVariable ("__project");
		}
	}
}

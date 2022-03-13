// 
// Reference.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2012-2014 Matthew Ward
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
using System.Reflection;
using System.Text;
using MonoDevelop.Core;
using MD = MonoDevelop.Projects;

namespace ICSharpCode.PackageManagement.EnvDTE
{
	public class Reference : MarshalByRefObject, global::EnvDTE.Reference
	{
		MD.ProjectReference referenceProjectItem;
		Project project;
		AssemblyName assemblyName;
		string publicKeyToken;

		public Reference (Project project, MD.ProjectReference referenceProjectItem)
		{
			this.project = project;
			this.referenceProjectItem = referenceProjectItem;
		}

		public string Name {
			get { return AssemblyName.Name; }
		}

		public string Path {
			get { return referenceProjectItem.HintPath; }
		}

		public void Remove ()
		{
			project.RemoveReference (referenceProjectItem);
			project.Save ();
		}

		public global::EnvDTE.Project SourceProject {
			get {
				return new Project (referenceProjectItem.OwnerProject as MD.DotNetProject);
			}
		}

		public string Identity {
			get { return AssemblyName.Name; }
		}

		public string PublicKeyToken {
			get {
				if (publicKeyToken == null) {
					byte[] token = AssemblyName.GetPublicKeyToken ();
					if (token.Length > 0) {
						publicKeyToken = GetPublicKeyToken (token);
					} else {
						publicKeyToken = String.Empty;
					}
				}
				return publicKeyToken;
			}
		}

		string GetPublicKeyToken (byte[] token)
		{
			var builder = new StringBuilder ();
			foreach (byte b in token) {
				builder.Append (b.ToString ("x2"));
			}
			return builder.ToString ();
		}

		public bool StrongName {
			get { return HasVersion () && HasPublicKeyToken (); }
		}

		bool HasVersion ()
		{
			return AssemblyName.Version != null;
		}

		bool HasPublicKeyToken ()
		{
			return !String.IsNullOrEmpty (PublicKeyToken);
		}

		AssemblyName AssemblyName {
			get {
				if (assemblyName == null) {
					assemblyName = GetAssemblyName ();
				}
				return assemblyName;
			}
		}

		AssemblyName GetAssemblyName ()
		{
			try {
				if (referenceProjectItem.ReferenceType == MD.ReferenceType.Package) {
					return new AssemblyName (referenceProjectItem.Reference);
				}

				// TODO: Fix me. ReflectionOnlyLoadFrom not supported.
				Assembly assembly = Assembly.ReflectionOnlyLoadFrom (referenceProjectItem.HintPath);
				if (assembly != null) {
					return assembly.GetName ();
				}
			} catch (Exception ex) {
				LoggingService.LogInternalError (ex);
			}
			return new AssemblyName (referenceProjectItem.Reference);
		}
	}
}

//
// MonoDevelopScriptPack.cs
//
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2014 Matthew Ward
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

using System.Collections.Generic;
using ScriptCs.Contracts;
using NuGet;
using ICSharpCode.PackageManagement.EnvDTE;

namespace MonoDevelop.PackageManagement
{
	public class NuGetScriptPack : IScriptPack
	{
		NuGetScriptPackContext context;
		Dictionary<string, object> variables = new Dictionary<string, object> ();

		public NuGetScriptPack (ILogger logger)
		{
			context = new NuGetScriptPackContext (logger);
		}

		public void AddVariable (string name, object value)
		{
			variables [name] = value;
		}

		public void RemoveVariable (string name)
		{
			variables.Remove (name);
		}

		public void Initialize (IScriptPackSession session)
		{
		}

		public IScriptPackContext GetContext ()
		{
			context.InstallPath = GetVariable<string> ("__rootPath");
			context.ToolsPath = GetVariable<string> ("__toolsPath");
			context.Package = GetVariable<IPackage> ("__package");
			context.Project = GetVariable<object> ("__project");
			context.DTE = new DTE ();
			return context;
		}

		T GetVariable<T> (string name)
			where T: class
		{
			object value = null;
			if (variables.TryGetValue (name, out value)) {
				return value as T;
			}

			return default(T);
		}

		public void Terminate ()
		{
		}
	}
}


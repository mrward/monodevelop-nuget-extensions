//
// ScriptCsHostContext.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
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
using ICSharpCode.PackageManagement.EnvDTE;
using NuGet;

namespace MonoDevelop.PackageManagement
{
	public class ScriptCsHostContext
	{
		public ScriptCsHostContext (ILogger logger, IDictionary<string, object> variables)
		{
			Logger = logger;
			ReadVariables (variables);
		}

		public ILogger Logger { get; private set; }
		public string InstallPath { get; private set; }
		public string ToolsPath { get; private set; }
		public IPackage Package { get; private set; }
		public dynamic Project { get; private set; }
		public dynamic DTE { get; private set; }

		void ReadVariables (IDictionary<string, object> variables)
		{
			InstallPath = GetVariable<string> (variables, "__rootPath");
			ToolsPath = GetVariable<string> (variables, "__toolsPath");
			Package = GetVariable<IPackage> (variables, "__package");
			Project = GetVariable<object> (variables, "__project");
			DTE = new DTE ();
		}

		T GetVariable<T> (IDictionary <string, object> variables, string name)
			where T: class
		{
			object value = null;
			if (variables.TryGetValue (name, out value)) {
				return value as T;
			}

			return default(T);
		}
	}
}


//
// ScriptCsSession.cs
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

using System;
using ICSharpCode.PackageManagement.Scripting;
using NuGet;

namespace MonoDevelop.PackageManagement
{
	public class ScriptCsSession : IPackageScriptSession
	{
		ScriptCsHost host;

		public ScriptCsSession (ILogger logger)
		{
			host = new ScriptCsHost (logger);
		}

		public void SetEnvironmentPath (string path)
		{
		}

		public string GetEnvironmentPath ()
		{
			return String.Empty;
		}

		public void AddVariable (string name, object value)
		{
			host.AddVariable (name, value);
		}

		public void RemoveVariable (string name)
		{
			host.RemoveVariable (name);
		}

		public void InvokeScript (string script)
		{
			string fileName = GetFileName (script);
			host.InvokeScript (fileName);
		}

		/// <summary>
		/// HACK: The script is PowerShell specific and is of the form:
		/// 
		/// "& '{0}' $__rootPath $__toolsPath $__package $__project"
		/// 
		/// So here we strip away everything apart from the filename inside the single quotes.
		/// </summary>
		static string GetFileName (string script)
		{
			int parametersLength = "' $__rootPath $__toolsPath $__package $__project".Length;
			int index = script.Length - parametersLength;
			string trimmedScript = script.Substring (0, index);
			trimmedScript = trimmedScript.Substring ("& '".Length);
			return trimmedScript;
		}
	}
}


//
// MonoPclCommandLine.cs
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
using System;
using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;
using System.IO;

namespace MonoDevelop.PackageManagement
{
	public class MonoPclCommandLine
	{
		MonoTargetRuntime monoRuntime;
		bool isMonoRuntime;

		public MonoPclCommandLine ()
			: this (
				Runtime.SystemAssemblyService.DefaultMonoRuntime as MonoTargetRuntime)
		{
		}

		public MonoPclCommandLine (MonoTargetRuntime monoRuntime)
		{
			this.monoRuntime = monoRuntime;

			List = true;
		}

		public string Command { get; set; }
		public string Arguments { get; private set; }
		public string WorkingDirectory { get; private set; }
		public bool List { get; set; }

		public void BuildCommandLine ()
		{
			GenerateMonoCommandLine ();
		}

		void GenerateMonoCommandLine ()
		{
			Arguments = String.Format (
				"--runtime=v4.0 \"{0}\" {1}",
				MonoPclExe.GetPath (),
				GetOptions ());

			Command = Path.Combine (monoRuntime.Prefix, "bin", "mono");
		}

		string GetOptions ()
		{
			if (List)
				return "list";
			return String.Empty;
		}

		public override string ToString ()
		{
			return String.Format ("{0} {1}", GetQuotedCommand (), Arguments);
		}

		string GetQuotedCommand ()
		{
			if (Command.Contains (" ")) {
				return String.Format ("\"{0}\"", Command);
			}
			return Command;
		}
	}
}
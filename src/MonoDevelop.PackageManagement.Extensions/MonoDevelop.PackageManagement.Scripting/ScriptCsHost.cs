//
// ScriptCsHost.cs
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

using ScriptCs.Engine.Mono;
using NuGet;

namespace MonoDevelop.PackageManagement
{
	public class ScriptCsHost : MonoHost
	{
		static NuGetScriptPackContext context;

		static ScriptCsHost ()
		{
			ScriptHost = new ScriptCsHostInfo ();
		}

		public static ScriptCsHostInfo ScriptHost { get; private set; }

		internal static void SetHost (NuGetScriptPack scriptPack, ILogger logger)
		{
			ScriptCsHost.context = (NuGetScriptPackContext)scriptPack.GetContext ();
			ScriptCsHost.Logger = logger;
		}

		public static IPackage Package {
			get { return context.Package; }
		}

		public static string InstallPath {
			get { return context.InstallPath; }
		}

		public static string ToolsPath {
			get { return context.ToolsPath; }
		}

		public static dynamic Project {
			get { return context.Project; }
		}

		public static dynamic DTE {
			get { return context.DTE; }
		}

		static ILogger Logger { get; set; }

		public static void Log (string message)
		{
			Logger.Log (MessageLevel.Info, message);
		}

		public static void LogDebug (string message)
		{
			Logger.Log (MessageLevel.Debug, message);
		}

		public static void LogError (string message)
		{
			Logger.Log (MessageLevel.Error, message);
		}

		public static void LogWarning (string message)
		{
			Logger.Log (MessageLevel.Warning, message);
		}
	}
}


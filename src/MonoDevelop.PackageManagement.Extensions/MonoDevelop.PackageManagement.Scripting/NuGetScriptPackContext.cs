//
// MonoDevelopScriptPackContext.cs
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
using System.Collections.Generic;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using NuGet;
using ScriptCs.Contracts;

namespace MonoDevelop.PackageManagement
{
	public class NuGetScriptPackContext : IScriptPackContext
	{
		public NuGetScriptPackContext (ILogger logger)
		{
			Logger = logger;
		}

		ILogger Logger { get; set; }

		public void Log (string message)
		{
			Logger.Log (MessageLevel.Info, message);
		}

		public void LogDebug (string message)
		{
			Logger.Log (MessageLevel.Debug, message);
		}

		public void LogError (string message)
		{
			Logger.Log (MessageLevel.Error, message);
		}

		public void LogWarning (string message)
		{
			Logger.Log (MessageLevel.Warning, message);
		}

		public Version NuGetVersion {
			get { return ICSharpCode.PackageManagement.Scripting.NuGetVersion.Version; }
		}

		public string HostName {
			get { return BrandingService.ApplicationName; }
		}

		public Version HostVersion {
			get { return IdeApp.Version; }
		}

		public string InstallPath { get; internal set; }
		public string ToolsPath { get; internal set; }
		public IPackage Package { get; internal set; }
		public dynamic Project { get; internal set; }
		public dynamic DTE { get; internal set; }
	}
}


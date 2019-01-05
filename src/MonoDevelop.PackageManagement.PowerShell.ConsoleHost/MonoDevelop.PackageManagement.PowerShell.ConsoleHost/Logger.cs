//
// Logger.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft
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

using System;
using System.IO;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost
{
	static class Logger
	{
		static string logFileName;

		public static void Initialize (string logDirectory)
		{
			if (!string.IsNullOrEmpty (logDirectory)) {
				Directory.CreateDirectory (logDirectory);
			}

			logFileName = string.Format ("{0}.{1}.log", "PowerShellHost", DateTime.Now.ToString ("yyyy-MM-dd__HH-mm-ss"));
			logFileName = Path.Combine (logDirectory, logFileName);
		}

		public static void Log (string format, params object[] args)
		{
			string message = string.Format (format, args);
			LogInternal (message);
		}

		static void LogInternal (string message)
		{
			string dateString = DateTime.Now.ToString ("u");
			string fullMessage = string.Format ("[{0}] {1}{2}", dateString, message, Environment.NewLine);
			File.AppendAllText (logFileName, fullMessage);
		}
	}
}

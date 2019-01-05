//
// PowerShellHost.cs
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
using System.Globalization;
using System.Management.Automation.Host;
using System.Threading;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost
{
	class PowerShellHost : PSHost
	{
		PowerShellUserInterfaceHost ui = new PowerShellUserInterfaceHost ();

		CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		Guid instanceId = Guid.NewGuid ();

		public override CultureInfo CurrentCulture => currentCulture;
		public override CultureInfo CurrentUICulture => currentUICulture;
		public override Guid InstanceId => instanceId;
		public override string Name => "Package Manager Host";
		public override PSHostUserInterface UI => ui;
		public override Version Version => new Version (1, 0);

		public override void EnterNestedPrompt ()
		{
			Logger.Log ("EnterNestedPrompt");
		}

		public override void ExitNestedPrompt ()
		{
			Logger.Log ("EnterNestedPrompt");
		}

		public override void NotifyBeginApplication ()
		{
			Logger.Log ("NotifyBeginApplication");
		}

		public override void NotifyEndApplication ()
		{
			Logger.Log ("NotifyEndApplication");
		}

		public override void SetShouldExit (int exitCode)
		{
			Logger.Log ("SetShouldExit {0}", exitCode);
		}
	}
}

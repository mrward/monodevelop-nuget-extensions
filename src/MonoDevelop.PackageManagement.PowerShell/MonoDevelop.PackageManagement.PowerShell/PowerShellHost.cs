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
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Threading;
using MonoDevelop.PackageManagement.Scripting;

namespace MonoDevelop.PackageManagement.PowerShell
{
	public class PowerShellHost : PSHost
	{
		PowerShellUserInterfaceHost ui;

		CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		Guid instanceId = Guid.NewGuid ();
		Version version;
		PSObject privateData;

		public PowerShellHost (
			IScriptingConsole scriptingConsole,
			Version version,
			object privateData)
		{
			//var hostCommandHandler = new PowerShellHostCommandHandler ();
			//privateData = new PSObject (hostCommandHandler);

			this.version = version;
			this.privateData = new PSObject (privateData);

			ui = new PowerShellUserInterfaceHost (scriptingConsole);
		}

		public override CultureInfo CurrentCulture => currentCulture;
		public override CultureInfo CurrentUICulture => currentUICulture;
		public override Guid InstanceId => instanceId;
		public override string Name => "Package Manager Host";
		public override PSHostUserInterface UI => ui;
		public override Version Version => version;

		public override PSObject PrivateData => privateData;

		public int MaxVisibleColumns {
			get { return ui.MaxVisibleColumns; }
			set { ui.MaxVisibleColumns = value; }
		}

		public override void EnterNestedPrompt ()
		{
		}

		public override void ExitNestedPrompt ()
		{
		}

		public override void NotifyBeginApplication ()
		{
		}

		public override void NotifyEndApplication ()
		{
		}

		public override void SetShouldExit (int exitCode)
		{
		}

		public void SetPropertyValueOnHost (string propertyName, object value)
		{
			PSPropertyInfo property = PrivateData.Properties[propertyName];
			if (property == null) {
				property = new PSNoteProperty (propertyName, value);
				PrivateData.Properties.Add (property);
			} else {
				property.Value = value;
			}
		}
	}
}

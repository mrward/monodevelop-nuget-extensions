//
// SendGreetingCommand.cs
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

using System.Management.Automation;

namespace MonoDevelop.PackageManagement.PowerShell.Cmdlets
{
	[Cmdlet (VerbsCommunications.Send, "Greeting")]
	public class SendGreetingCommand : Cmdlet
	{
		// Declare the parameters for the cmdlet.
		[Parameter (Mandatory = true)]
		public string Name { get; set; }

		// Overide the ProcessRecord method to process
		// the supplied user name and write out a
		// greeting to the user by calling the WriteObject
		// method.
		protected override void ProcessRecord ()
		{
			WriteObject ("Hello " + Name + "!");
		}
	}
}

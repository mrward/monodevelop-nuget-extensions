//
// LazyCommandExpansion.cs
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
using System.Threading;
using System.Threading.Tasks;
using NuGetConsole;

namespace MonoDevelop.PackageManagement.Scripting
{
	/// <summary>
	/// Creates the ICommandExpansion lazily just before it is needed. It requires the
	/// JsonRpc instance to be created which is not available until the PowerShell
	/// host is initialized.
	/// </summary>
	class LazyCommandExpansion : ICommandExpansion
	{
		Lazy<ICommandExpansion> lazyCommandExpansion;
		ICommandExpansion commandExpansion;

		public LazyCommandExpansion (Func<ICommandExpansion> createCommandExpansion)
		{
			lazyCommandExpansion = new Lazy<ICommandExpansion> (createCommandExpansion);
		}

		public Task<SimpleExpansion> GetExpansionsAsync (
			string line,
			int caretIndex,
			CancellationToken token)
		{
			return CommandExpansion.GetExpansionsAsync (line, caretIndex, token);
		}

		public ICommandExpansion CommandExpansion {
			get {
				if (commandExpansion != null) {
					return commandExpansion;
				}
				return lazyCommandExpansion.Value;
			}
			set {
				commandExpansion = value;
			}
		}
	}
}

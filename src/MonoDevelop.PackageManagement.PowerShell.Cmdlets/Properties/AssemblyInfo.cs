﻿//
// AssemblyInfo.cs
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

using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle ("MonoDevelop.PackageManagement.PowerShell.Cmdlets")]
[assembly: AssemblyDescription ("Package Management PowerShell Cmdlets for MonoDevelop")]
[assembly: AssemblyProduct ("MonoDevelop.PackageManagement")]
[assembly: AssemblyCopyright ("Copyright (C) 2019 Matthew Ward")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

// This sets the default COM visibility of types in the assembly to invisible.
// If you need to expose a type to COM, use [ComVisible(true)] on that type.
[assembly: ComVisible (false)]

// Need to fix CA1416 build warning.
// This call site is reachable on all platforms. 'NSLayoutConstraint.Active' is only supported on: 'ios' 10.0 and later,
// 'maccatalyst' 10.0 and later, 'macOS/OSX' 10.14 and later, 'tvos' 10.0 and later. (CA1416))
// https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1416
[assembly: SupportedOSPlatform ("macos10.15")]

// The assembly version has following format :
//
// Major.Minor.Build.Revision
//
// You can specify all the values or you can use the default the Revision and 
// Build Numbers by using the '*' as shown below:
[assembly: AssemblyVersion ("0.31.1")]

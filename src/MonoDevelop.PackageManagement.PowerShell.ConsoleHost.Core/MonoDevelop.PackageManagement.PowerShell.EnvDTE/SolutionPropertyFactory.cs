﻿//
// SolutionPropertyFactory.cs
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

using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.PackageManagement.PowerShell.EnvDTE
{
	class SolutionPropertyFactory : IPropertyFactory
	{
		readonly Solution solution;

		public SolutionPropertyFactory (Solution solution)
		{
			this.solution = solution;
		}

		public Property CreateProperty (string name)
		{
			return new SolutionProperty (solution, name);
		}

		public IEnumerator<Property> GetEnumerator ()
		{
			List<Property> properties = GetProperties ().ToList ();
			return properties.GetEnumerator ();
		}

		IEnumerable<Property> GetProperties ()
		{
			return new Property[0];
			//foreach (string propertyName in project.GetAllPropertyNames ()) {
			//	yield return CreateProperty (propertyName);
			//}
		}
	}
}
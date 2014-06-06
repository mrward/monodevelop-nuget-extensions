//
// InstallPackageCommand.cs
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
using NuGet;

namespace MonoDevelop.PackageManagement
{
	public class InstallPackageCommand
	{
		public InstallPackageCommand (string text)
		{
			Text = RemoveWhitespace (text);
			Parse (Text);
		}

		public string PackageId { get; private set; }
		public string Text { get; private set; }
		public string Version { get; private set; }
		public bool IsValid { get; private set; }

		public bool IsPackageVersionSearch {
			get { return !String.IsNullOrEmpty (PackageId); }
		}

		public bool HasVersion ()
		{
			if (String.IsNullOrEmpty (Version))
				return false;

			return IsValidVersionNumber ();
		}

		bool IsValidVersionNumber ()
		{
			SemanticVersion version = null;
			return SemanticVersion.TryParse (Version, out version);
		}

		public SemanticVersion GetVersion ()
		{
			if (String.IsNullOrEmpty (Version))
				return null;

			return new SemanticVersion (Version);
		}
	
		string RemoveWhitespace (string text)
		{
			if (String.IsNullOrWhiteSpace (text))
				return null;

			return text;
		}

		void Parse (string text)
		{
			PackageId = String.Empty;
			Version = String.Empty;

			if (text == null)
				return;

			IsValid = true;

			string[] parts = text.Split (new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length > 0) {
				PackageId = parts [0].Trim ();
			}

			if (parts.Length < 3)
				return;

			if (!IsVersionOption (parts [1]))
				return;

			Version = parts [2].Trim ();
		}

		string GetUsage ()
		{
			return "Usage: PackageId [-version number]";
		}

		bool IsVersionOption (string option)
		{
			return
				IsMatch (option, "-v") ||
				IsMatch (option, "-ver") ||
				IsMatch (option, "-version");
		}

		static bool IsMatch (string a, string b)
		{
			return String.Equals (a, b, StringComparison.OrdinalIgnoreCase);
		}
	}
}


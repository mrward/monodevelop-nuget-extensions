//
// PowerShellHostPrivateData.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2022 Microsoft
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

namespace MonoDevelop.PackageManagement.PowerShell
{
	public class PowerShellHostPrivateData
	{
		public PowerShellHostPrivateData ()
		{
		}

		internal PowerShellUserInterfaceHost UI { get; set; }

		public ConsoleColor FormatAccentColor {
			get {
				return UI.FormatAccentColor;
			}

			set {
				UI.FormatAccentColor = value;
			}
		}

		public ConsoleColor ErrorAccentColor {
			get {
				return UI.ErrorAccentColor;
			}

			set {
				UI.ErrorAccentColor = value;
			}
		}

		public ConsoleColor ErrorForegroundColor {
			get {
				return UI.ErrorForegroundColor;
			}

			set {
				UI.ErrorForegroundColor = value;
			}
		}

		public ConsoleColor ErrorBackgroundColor {
			get {
				return UI.ErrorBackgroundColor;
			}

			set {
				UI.ErrorBackgroundColor = value;
			}
		}

		public ConsoleColor WarningForegroundColor {
			get {
				return UI.WarningForegroundColor;
			}

			set {
				UI.WarningForegroundColor = value;
			}
		}

		public ConsoleColor WarningBackgroundColor {
			get {
				return UI.WarningBackgroundColor;
			}

			set {
				UI.WarningBackgroundColor = value;
			}
		}

		public ConsoleColor DebugForegroundColor {
			get {
				return UI.DebugForegroundColor;
			}

			set {
				UI.DebugForegroundColor = value;
			}
		}

		public ConsoleColor DebugBackgroundColor {
			get {
				return UI.DebugBackgroundColor;
			}

			set {
				UI.DebugBackgroundColor = value;
			}
		}

		public ConsoleColor VerboseForegroundColor {
			get {
				return UI.VerboseForegroundColor;
			}

			set {
				UI.VerboseForegroundColor = value;
			}
		}

		public ConsoleColor VerboseBackgroundColor {
			get {
				return UI.VerboseBackgroundColor;
			}

			set {
				UI.VerboseBackgroundColor = value;
			}
		}

		public ConsoleColor ProgressForegroundColor {
			get {
				return UI.ProgressForegroundColor;
			}

			set {
				UI.ProgressForegroundColor = value;
			}
		}

		public ConsoleColor ProgressBackgroundColor {
			get {
				return UI.ProgressBackgroundColor;
			}

			set {
				UI.ProgressBackgroundColor = value;
			}
		}
	}
}


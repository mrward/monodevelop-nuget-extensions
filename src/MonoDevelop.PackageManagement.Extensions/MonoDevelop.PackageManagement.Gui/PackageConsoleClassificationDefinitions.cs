//
// PackageConsoleClassificationDefinitions.cs
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

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace MonoDevelop.PackageManagement
{
    internal sealed class PackageConsoleClassificationDefinitions
    {
        [Export (typeof (ClassificationTypeDefinition))]
        [Name (PackageConsoleClassificationTypes.Error)]
        internal ClassificationTypeDefinition PackageConsoleErrorClassificationType { get; set; }

        [Export (typeof (EditorFormatDefinition))]
        [UserVisible (true)]
        [ClassificationType (ClassificationTypeNames = PackageConsoleClassificationTypes.Error)]
        [Name (nameof (PackageConsoleErrorClassificationFormatDefinition))]
        sealed class PackageConsoleErrorClassificationFormatDefinition : ClassificationFormatDefinition
        {
            internal PackageConsoleErrorClassificationFormatDefinition ()
            {
                BackgroundColor = Colors.Red;
                ForegroundColor = Colors.White;
                DisplayName = nameof (PackageConsoleErrorClassificationFormatDefinition);
            }
        }

        [Export (typeof (ClassificationTypeDefinition))]
        [Name (PackageConsoleClassificationTypes.Warning)]
        internal ClassificationTypeDefinition PackageConsoleWarningClassificationType { get; set; }

        [Export (typeof (EditorFormatDefinition))]
        [UserVisible (true)]
        [ClassificationType (ClassificationTypeNames = PackageConsoleClassificationTypes.Warning)]
        [Name (nameof (PackageConsoleWarningClassificationFormatDefinition))]
        sealed class PackageConsoleWarningClassificationFormatDefinition : ClassificationFormatDefinition
        {
            internal PackageConsoleWarningClassificationFormatDefinition ()
            {
                BackgroundColor = Colors.Yellow;
                ForegroundColor = Colors.Black;
                DisplayName = nameof (PackageConsoleWarningClassificationFormatDefinition);
            }
        }

        [Export (typeof (ClassificationTypeDefinition))]
        [Name (PackageConsoleClassificationTypes.Debug)]
        internal ClassificationTypeDefinition PackageConsoleDebugClassificationType { get; set; }

        [Export (typeof (EditorFormatDefinition))]
        [UserVisible (true)]
        [ClassificationType (ClassificationTypeNames = PackageConsoleClassificationTypes.Debug)]
        [Name (nameof (PackageConsoleDebugClassificationFormatDefinition))]
        sealed class PackageConsoleDebugClassificationFormatDefinition : ClassificationFormatDefinition
        {
            internal PackageConsoleDebugClassificationFormatDefinition ()
            {
                ForegroundColor = Colors.DarkGray;
                DisplayName = nameof (PackageConsoleDebugClassificationFormatDefinition);
            }
        }
    }
}


//
// PackageConsoleClassifier.cs
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
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace MonoDevelop.PackageManagement
{
	class PackageConsoleClassifier : IClassifier
	{
		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

		readonly ITextBuffer textBuffer;
		readonly PackageConsoleClassifierProvider provider;
		PackageConsoleViewController controller;

		public PackageConsoleClassifier (
			ITextBuffer textBuffer,
			PackageConsoleClassifierProvider provider)
		{
			this.textBuffer = textBuffer;
			this.provider = provider;
		}

		bool HasController {
			get { return Controller != null; }
		}

		PackageConsoleViewController Controller {
			get {
				if (controller == null) {
					textBuffer.Properties.TryGetProperty (typeof (PackageConsoleViewController), out controller);

					if (controller != null) {
						controller.ColoredSpanAdded += ColoredSpanAdded;
					}
				}
				return controller;
			}
		}

		void ColoredSpanAdded (object sender, PackageConsoleClassificationTypeSpanInfoEventArgs e)
		{
			ClassificationChanged?.Invoke (this, new ClassificationChangedEventArgs (e.SpanInfo.SnapshotSpan));
		}

		public IList<ClassificationSpan> GetClassificationSpans (SnapshotSpan span)
		{
			if (!HasController)
			{
				return null;
			}

			ITextSnapshot snapshot = span.Snapshot;

			var classifications = new List<ClassificationSpan>();

			foreach (PackageConsoleClassificationTypeSpanInfo spanInfo in controller.ColoredSpans.Overlap (span)) {
				if (spanInfo.Span.OverlapsWith (span)) {
					IClassificationType classificationType = provider.
						ClassificationTypeRegistryService.
						GetClassificationType (spanInfo.ClassificationTypeName);

					// Constrain the length to avoid ArgumentOutOfRangeException if the colored
					// snapshot ends after the span being checked for classifications.
					int spanStart = spanInfo.Span.Start;

					int spanLength = spanInfo.Span.Length;
					if (spanStart + spanLength > snapshot.Length) {
						spanLength = snapshot.Length - spanStart;
					}

					var snapshotSpan = new SnapshotSpan (snapshot, spanStart, spanLength);

					var classificationSpan = new ClassificationSpan (snapshotSpan, classificationType);

					classifications.Add (classificationSpan);
				}
			}

			return classifications;
		}
	}
}


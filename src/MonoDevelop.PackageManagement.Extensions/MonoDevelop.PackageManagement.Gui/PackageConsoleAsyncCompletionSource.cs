//
// PackageConsoleAsyncCompletionSource.cs
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
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Components;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using MonoDevelop.Core;
using NuGetConsole;

namespace MonoDevelop.PackageManagement
{
	/// <summary>
	/// Implementing IAsyncCompletionUniversalSource since this allows the completion items to
	/// define their own ApplicableToSpan. Implementing only the IAsyncCompletionSource
	/// results in the applicableToSpan being fixed when returned from InitializeCompletion.
	/// InitializeCompletion is synchronous so we do not want to run the command expansion there
	/// so we cannot use IAsyncCompletionSource. IAsyncCompletionUniversalSource being
	/// implemented means GetCompletionContextAsync is called without InitializeCompletion
	/// being called and the ApplicableToSpan set on each CompletionItem will be used.
	/// </summary>
	class PackageConsoleAsyncCompletionSource : IAsyncCompletionUniversalSource
	{
		const int TabExpansionTimeout = 3; // seconds.

		readonly ITextView textView;
		readonly ICommandExpansion commandExpansion;

		public PackageConsoleAsyncCompletionSource (ITextView textView)
		{
			this.textView = textView;
			textView.Properties.TryGetProperty (typeof (ICommandExpansion), out commandExpansion);
		}

		/// <summary>
		/// Not called when IAsyncCompletionUniversalSource is implemented. 
		/// </summary>
		public Task<CompletionContext> GetCompletionContextAsync (
			IAsyncCompletionSession session,
			CompletionTrigger trigger,
			SnapshotPoint triggerLocation,
			SnapshotSpan applicableToSpan,
			CancellationToken token)
		{
			throw new NotImplementedException ("GetCompletionContextAsync overload should not be called");
		}

		public async Task<CompletionContext> GetCompletionContextAsync (
			CompletionTrigger trigger,
			SnapshotPoint triggerLocation,
			CancellationToken token)
		{
			if (!IsSupported (trigger)) {
				return CompletionContext.Empty;
			}

			ITextSnapshotLine line = triggerLocation.GetContainingLine ();
			if (line == null) {
				return CompletionContext.Empty;
			}

			int promptLength = GetPromptLength ();
			SnapshotPoint lineStart = line.Start.Add (promptLength);
			if (lineStart.Position > line.End.Position) {
				return CompletionContext.Empty;
			}

			string text = triggerLocation.Snapshot.GetText (lineStart.Position, line.End.Position - lineStart.Position);

			int caretIndex = triggerLocation.Position - lineStart.Position;
			if (caretIndex < 0) {
				return CompletionContext.Empty;
			}

			var cancellationTokenSource = new CancellationTokenSource (TabExpansionTimeout * 1000);

			using var registration = token.Register(() => {
				cancellationTokenSource.Cancel();
			});

			SimpleExpansion simpleExpansion = await TryGetExpansionsAsync (
					text,
					caretIndex,
					cancellationTokenSource.Token);

			if (simpleExpansion == null || !simpleExpansion.Expansions.Any ()) {
				return CompletionContext.Empty;
			}

			SnapshotPoint snapshot = lineStart.Add (simpleExpansion.Start);
			var span = new SnapshotSpan (snapshot, simpleExpansion.Length);

			var completionItems = new List<CompletionItem> ();
			foreach (string expansionText in simpleExpansion.Expansions) {
				var item = new CompletionItem (expansionText, this, span);
				completionItems.Add (item);
			}

			var context = new CompletionContext (completionItems.ToImmutableArray ());
			return context;
		}

		/// <summary>
		/// Get completion on pressing tab or when the dot character is typed.
		/// </summary>
		bool IsSupported (CompletionTrigger trigger)
		{
			if (commandExpansion == null) {
				return false;
			}

			if (trigger.Reason == CompletionTriggerReason.InvokeAndCommitIfUnique) {
				return true;
			}

			return false;
		}

		int GetPromptLength()
		{
			var controller = textView.Properties[typeof (ConsoleViewController)] as ConsoleViewController;
			if (controller != null) {
				return controller.InputLineStart;
			}
			return 0;
		}

		public Task<object> GetDescriptionAsync (
			IAsyncCompletionSession session,
			CompletionItem item,
			CancellationToken token)
		{
			return Task.FromResult<object> (null);
		}

		public CompletionContinuation HandleTypedChar (
			IAsyncCompletionSession session,
			CompletionItem selectedItem,
			SnapshotPoint location,
			char typedChar,
			CancellationToken token)
		{
			return CompletionContinuation.Continue;
		}

		/// <summary>
		/// Not called when IAsyncCompletionUniversalSource is implemented.
		/// </summary>
		public CompletionStartData InitializeCompletion (
			CompletionTrigger trigger,
			SnapshotPoint triggerLocation,
			CancellationToken token)
		{
			throw new NotImplementedException ("InitializeCompletion should not be called");
		}

		public CommitResult TryCommit (IAsyncCompletionSession session, ITextBuffer buffer, CompletionItem item, char typedChar, CancellationToken token)
		{
			return CommitResult.Unhandled;
		}

		async Task<SimpleExpansion> TryGetExpansionsAsync (
			string line,
			int caretIndex,
			CancellationToken token)
		{
			try {
				return await commandExpansion.GetExpansionsAsync (line, caretIndex, token);
			} catch (OperationCanceledException) {
				return null;
			} catch (Exception ex) {
				LoggingService.LogError ("GetExpansionsAsync error.", ex);
				return null;
			}
		}
	}
}

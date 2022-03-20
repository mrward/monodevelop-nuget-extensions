//
// PackageConsoleTabKeyHandler.cs
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
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;

namespace MonoDevelop.PackageManagement
{
	[Name (nameof (PackageConsoleTabKeyHandler))]
	[ContentType ("text")]
	[Export (typeof (ICommandHandler))]
	[Order (After = PredefinedCompletionNames.CompletionCommandHandler)]
	[TextViewRole (nameof (PackageConsoleViewController))]
	class PackageConsoleTabKeyHandler : ICommandHandler<TabKeyCommandArgs>
	{
		public string DisplayName => nameof (PackageConsoleTabKeyHandler);

		readonly IAsyncCompletionBroker completionBroker;

		[ImportingConstructor]
		public PackageConsoleTabKeyHandler (IAsyncCompletionBroker completionBroker)
		{
			this.completionBroker = completionBroker;

			completionBroker.CompletionTriggered += CompletionBrokerCompletionTriggered;
		}

		/// <summary>
		/// This is a bit clumsy. However this is the only way to open the completion list window
		/// when the completion generation is async. There is no way to register a callback
		/// with the broker so we have to use an event handler.
		/// </summary>
		void CompletionBrokerCompletionTriggered (object sender, CompletionTriggeredEventArgs args)
		{
			if (!trigger.HasValue) {
				return;
			}
			if (!token.HasValue || token.Value.IsCancellationRequested) {
				return;
			}

			var sessionOperations = args.CompletionSession as IAsyncCompletionSessionOperations;
			if (sessionOperations != null) {
				ITextSnapshot snapshotBeforeEdit = args.TextView.TextSnapshot;
				if (snapshotBeforeEdit.Version == trigger.Value.ViewSnapshotBeforeTrigger.Version) {
					SnapshotPoint location = args.TextView.Caret.Position.BufferPosition;
					sessionOperations.InvokeAndCommitIfUnique (trigger.Value, location, token.Value);
				}
			}
			trigger = null;
			token = null;
		}

		CancellationToken? token;
		CompletionTrigger? trigger;

		public bool ExecuteCommand (TabKeyCommandArgs args, CommandExecutionContext executionContext)
		{
			if (completionBroker == null) {
				return true;
			}

			ITextSnapshot snapshotBeforeEdit = args.TextView.TextSnapshot;
			trigger = new CompletionTrigger (CompletionTriggerReason.InvokeAndCommitIfUnique, args.TextView.TextSnapshot);
			SnapshotPoint location = args.TextView.Caret.Position.BufferPosition;
			token = executionContext.OperationContext.UserCancellationToken;

			IAsyncCompletionSession session = completionBroker.TriggerCompletion (args.TextView, trigger.Value, location, token.Value);

			return true;
		}

		public CommandState GetCommandState (TabKeyCommandArgs args)
		{
			return CommandState.Available;
		}
	}
}

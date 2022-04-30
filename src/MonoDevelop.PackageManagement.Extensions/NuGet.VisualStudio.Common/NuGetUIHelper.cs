// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.Threading;

namespace NuGet.VisualStudio
{
	public static class NuGetUIThreadHelper
	{
		public static JoinableTaskFactory JoinableTaskFactory {
			get {
				return MonoDevelop.Core.Runtime.JoinableTaskFactory;
			}
		}
	}
}
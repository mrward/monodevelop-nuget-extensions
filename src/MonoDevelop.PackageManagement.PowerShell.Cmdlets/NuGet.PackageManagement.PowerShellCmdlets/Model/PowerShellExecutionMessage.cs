// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
	public class Message
	{
	}

	public class ExecutionCompleteMessage : Message
	{
	}

	public class LogMessage : Message
	{
		public LogMessage (MessageLevel level, string content)
		{
			Level = level;
			Content = content;
		}

		public MessageLevel Level { get; set; }

		public string Content { get; set; }
	}

	public class ScriptMessage : Message
	{
		public ScriptMessage (
			string scriptPath,
			string installPath,
			PackageIdentity identity,
			global::EnvDTE.Project project)
		{
			ScriptPath = scriptPath;
			InstallPath = installPath;
			Identity = identity;
			Project = project;

			EndSemaphore = new Semaphore (0, int.MaxValue);
		}

		public string ScriptPath { get; set; }
		public string InstallPath { get; }
		public PackageIdentity Identity { get; }
		public global::EnvDTE.Project Project { get; }

		public Semaphore EndSemaphore { get; private set; }
		public Exception Exception { get; set; }
	}

	/// <summary>
	/// A message inserted into the Package Manager Console to signal that all messages in front of
	/// this message in the <see cref="BlockingCollection{T}"/> should be flushed before
	/// continuing.
	/// </summary>
	public class FlushMessage : Message
	{
	}
}
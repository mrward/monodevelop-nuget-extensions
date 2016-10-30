// 
// ConsoleHostFileConflictResolver.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using MonoDevelop.PackageManagement;
using NuGet.ProjectManagement;
using MonoDevelop.Core;

namespace ICSharpCode.PackageManagement.Scripting
{
	internal class ConsoleHostFileConflictResolver : IConsoleHostFileConflictResolver
	{
		IPackageManagementEvents packageEvents;
		FileConflictAction? conflictResolution;
		FileConflictResolver fileConflictResolver;
		
		public ConsoleHostFileConflictResolver (
			IPackageManagementEvents packageEvents,
			FileConflictAction? fileConflictAction)
		{
			this.packageEvents = packageEvents;
			fileConflictResolver = new FileConflictResolver ();

			if (fileConflictAction.HasValue) {
				conflictResolution = GetFileConflictResolution (fileConflictAction.Value);
			}
			packageEvents.ResolveFileConflict += ResolveFileConflict;
		}
		
		void ResolveFileConflict (object sender, ResolveFileConflictEventArgs e)
		{
			if (conflictResolution.HasValue) {
				e.Resolution = conflictResolution.Value;
			} else {
				Runtime.RunInMainThread (() => {
					e.Resolution = fileConflictResolver.ResolveFileConflict (e.Message);
				}).Wait ();
			}
		}
		
		FileConflictAction GetFileConflictResolution (FileConflictAction fileConflictAction)
		{
			switch (fileConflictAction) {
				case FileConflictAction.Overwrite:
					return FileConflictAction.Overwrite;
				default:
					return FileConflictAction.Ignore;
			}
		}
		
		public void Dispose ()
		{
			packageEvents.ResolveFileConflict -= ResolveFileConflict;
		}
	}
}

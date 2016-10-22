////
//// PackageActionRunner2.cs
////
//// Author:
////       Matt Ward <matt.ward@xamarin.com>
////
//// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
////
//// Permission is hereby granted, free of charge, to any person obtaining a copy
//// of this software and associated documentation files (the "Software"), to deal
//// in the Software without restriction, including without limitation the rights
//// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//// copies of the Software, and to permit persons to whom the Software is
//// furnished to do so, subject to the following conditions:
////
//// The above copyright notice and this permission notice shall be included in
//// all copies or substantial portions of the Software.
////
//// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//// THE SOFTWARE.
////
//
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using MonoDevelop.Core;
//
//namespace MonoDevelop.PackageManagement
//{
//	internal class PackageActionRunner2 : IPackageActionRunner
//	{
//		public void Run (IEnumerable<IPackageAction> actions)
//		{
//			var progressMessage = CreateProgressMessage (actions);
//			PackageManagementServices.BackgroundPackageActionRunner.Run (progressMessage, actions);
//		}
//
//		public void Run (IPackageAction action)
//		{
//			Run (new [] { action });
//		}
//
//		ProgressMonitorStatusMessage CreateProgressMessage (IEnumerable<IPackageAction> actions)
//		{
//			if (actions.Count () > 1) {
//				return CreateProgressMessageForMultipleActions (actions.Count ());
//			}
//			return CreateProgressMessage (actions.FirstOrDefault ());
//		}
//
//		ProgressMonitorStatusMessage CreateProgressMessageForMultipleActions (int count)
//		{
//			return new ProgressMonitorStatusMessage (
//				GettextCatalog.GetString ("Managing {0} packages...", count),
//				GettextCatalog.GetString ("{0} packages successfully managed.", count),
//				GettextCatalog.GetString ("Could not manage packages."),
//				GettextCatalog.GetString ("{0} packages managed with warnings.", count)
//			);
//		}
//
//		ProgressMonitorStatusMessage CreateProgressMessage (IPackageAction action)
//		{
//			var updateAction = action as UpdatePackageAction;
//			if (updateAction != null) {
//				return ProgressMonitorStatusMessageFactory.CreateUpdatingSinglePackageMessage (updateAction.GetPackageId ());
//			}
//
//			var uninstallAction = action as UninstallPackageAction;
//			if (uninstallAction != null) {
//				return ProgressMonitorStatusMessageFactory.CreateRemoveSinglePackageMessage (uninstallAction.GetPackageId ());
//			}
//
//			var processPackageAction = action as ProcessPackageAction;
//			return ProgressMonitorStatusMessageFactory.CreateInstallingSinglePackageMessage (processPackageAction.GetPackageId ());
//		}
//	}
//}
//

﻿//
// ItemOperationsMessageHandler.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft
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
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace MonoDevelop.PackageManagement.EnvDTE
{
	class ItemOperationsMessageHandler
	{
		[JsonRpcMethod (Methods.ItemOperationsNavigateName)]
		public void OnNavigate (JToken arg)
		{
			try {
				var navigateMessage = arg.ToObject<ItemOperationsNavigateParams> ();
				DesktopService.OpenFile (navigateMessage.Url);
			} catch (Exception ex) {
				LoggingService.LogError ("OnNavigate error: {0}", ex);
			}
		}

		[JsonRpcMethod (Methods.ItemOperationsOpenFileName)]
		public void OnOpenFile (JToken arg)
		{
			try {
				var message = arg.ToObject<ItemOperationsOpenFileParams> ();
				Runtime.RunInMainThread (() => {
					OpenFile (new FilePath (message.FileName));
				}).Ignore ();
			} catch (Exception ex) {
				LoggingService.LogError ("OnNavigate error: {0}", ex);
			}
		}

		void OpenFile (FilePath filePath)
		{
			IdeApp.Workbench.OpenDocument (filePath, null, true).Ignore ();
		}
	}
}
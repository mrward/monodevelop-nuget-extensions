﻿//
// NuGetConfigFileNodeBuilder.cs
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
using MonoDevelop.Ide.Gui.Components;

namespace MonoDevelop.PackageManagement.NodeBuilders
{
	class NuGetConfigFileNodeBuilder : TypeNodeBuilder
	{
		public override Type NodeDataType {
			get { return typeof (NuGetConfigFileNode); }
		}

		public override Type CommandHandlerType {
			get { return typeof (NuGetConfigFileNodeCommandHandler); }
		}

		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			var node = (NuGetConfigFileNode)dataObject;
			return node.Name;
		}

		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, NodeInfo nodeInfo)
		{
			var node = (NuGetConfigFileNode)dataObject;
			nodeInfo.Label = node.Name;
			nodeInfo.Icon = Context.GetIcon (node.IconId);
		}

		public override int GetSortIndex (ITreeNavigator node)
		{
			var fileNode = (NuGetConfigFileNode)node.DataItem;
			return fileNode.Index;
		}
	}
}


//
// VariableNodeBuilder.cs
//
// Authors:
//   Marcos David Marin Amador <MarcosMarin@gmail.com>
//
// Copyright (C) 2007 Marcos David Marin Amador
//
//
// This source code is licenced under The MIT License:
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

using System;
using System.IO;

using Mono.Addins;

using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Components;

using CBinding.Parser;

namespace CBinding.Navigation
{
	public class VariableNodeBuilder : TypeNodeBuilder
	{
		public override Type NodeDataType {
			get { return typeof(Variable); }
		}
		
		public override Type CommandHandlerType {
			get { return typeof(LanguageItemCommandHandler); }
		}
		
		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			return ((Variable)dataObject).Name;
		}
		
		public override void BuildNode (ITreeBuilder treeBuilder,
		                                object dataObject,
		                                NodeInfo nodeInfo)
		{
			Variable v = (Variable)dataObject;
				
			nodeInfo.Label = v.Name;
			
			switch (v.Access)
			{
			case AccessModifier.Public:
				nodeInfo.Icon = Context.GetIcon (Stock.Field);
				break;
			case AccessModifier.Protected:
				nodeInfo.Icon = Context.GetIcon (Stock.ProtectedField);
				break;
			case AccessModifier.Private:
				nodeInfo.Icon = Context.GetIcon (Stock.PrivateField);
				break;
			}
		}
		
		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			return false;
		}
		
		public override int CompareObjects (ITreeNavigator thisNode, ITreeNavigator otherNode)
		{
			if (otherNode.DataItem is Function)
				return 1;
			else
				return -1;
		}
	}
}

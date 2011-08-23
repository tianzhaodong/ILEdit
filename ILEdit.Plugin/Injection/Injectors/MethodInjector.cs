﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILEdit.Injection;
using ICSharpCode.ILSpy.TreeNodes;
using Mono.Cecil;
using System.Windows;
using Mono.Cecil.Cil;

namespace ILEdit.Injection.Injectors
{
    /// <summary>
    /// Method injector
    /// </summary>
    public class MethodInjector : IInjector
    {
        #region Properties

        public string Name
        {
            get { return "Method"; }
        }

        public string Description
        {
            get { return "Injects a new empty method"; }
        }

        public System.Windows.Media.ImageSource Icon
        {
            get { return new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/ILSpy;component/Images/Method.png")); }
        }

        public bool NeedsMember
        {
            get { return false; }
        }

        public Predicate<Mono.Cecil.IMetadataTokenProvider> MemberFilter
        {
            get { return null; }
        }

        public TokenType SelectableMembers
        {
            get { return TokenType.TypeDef; }
        }

        #endregion

        public bool CanInjectInNode(ICSharpCode.ILSpy.TreeNodes.ILSpyTreeNode node)
        {
            //Try-cast
            var memberNode = node as IMemberTreeNode;
            var type = memberNode == null ? null : (memberNode.Member as TypeDefinition);

            //Can inject only in types
            return type != null;
        }

        public void Inject(ICSharpCode.ILSpy.TreeNodes.ILSpyTreeNode node, string name, IMetadataTokenProvider member)
        {
            //Type
            var type = ((IMemberTreeNode)node).Member as TypeDefinition;

            //Creates the method definition
            var method = new MethodDefinition(
                name,
                MethodAttributes.Public,
                type.Module.TypeSystem.Void
            )
            {
                MetadataToken = new MetadataToken(TokenType.Method, ILEdit.GlobalContainer.GetFreeRID(type.Module))
            };

            //Adds the method to the type
            type.Methods.Add(method);
            if (node is TypeTreeNode)
            {
                node.Children.Add(new ILEditTreeNode(method, false));
                TreeHelper.SortChildren((TypeTreeNode)node);
            }
            else if (node is ILEditTreeNode)
            {
                ((ILEditTreeNode)node).RefreshChildren();
            }
        }
    }
}
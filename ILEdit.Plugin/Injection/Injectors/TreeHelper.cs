﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.TreeNodes;
using Mono.Cecil;

namespace ILEdit.Injection.Injectors
{
    /// <summary>
    /// Provides a set of methods to help the managing of tree nodes
    /// </summary>
    internal static class TreeHelper
    {
        #region SortChildren

        /// <summary>
        /// Sorts the children of a ModuleTreeNode
        /// </summary>
        /// <param name="node"></param>
        public static void SortChildren(ModuleTreeNode node)
        {
            //Groups the children by type and performs ordering
            var ordered =
                node.Children
                .GroupBy(x => x is NamespaceTreeNode)
                .OrderBy(x => x.Key)
                .SelectMany(x => x.OrderBy(y => y.Text.ToString()))
                .ToArray();

            //Clears the children
            node.Children.Clear();

            //Readds the children
            foreach (var x in ordered)
                node.Children.Add(x);
        }

        /// <summary>
        /// Sorts the children of a NamespaceTreeNode
        /// </summary>
        /// <param name="node"></param>
        public static void SortChildren(NamespaceTreeNode node)
        {
            //Groups the children by type and performs ordering
            var ordered =
                node.Children
                .OrderBy(x => x.Text.ToString())
                .ToArray();

            //Clears the children
            node.Children.Clear();

            //Readds the children
            foreach (var x in ordered)
                node.Children.Add(x);
        }

        /// <summary>
        /// Sorts the children of a TypeTreeNode
        /// </summary>
        /// <param name="node"></param>
        public static void SortChildren(TypeTreeNode node)
        {
            //Array for the type ordering
            var typeOrder = new List<Type>(new Type[] {
                typeof(TypeTreeNode),
                typeof(FieldTreeNode),
                typeof(PropertyTreeNode),
                typeof(EventTreeNode),
                typeof(MethodTreeNode)
            });

            //Groups the childen by type
            var ordered =
                node.Children
                .GroupBy(x => typeOrder.IndexOf(x.GetType()))
                .OrderBy(x => x.Key)
                .SelectMany(x => x.OrderBy(y => y.Text.ToString()))
                .ToArray();

            //Clears the children
            node.Children.Clear();

            //Readds the children
            foreach (var x in ordered)
                node.Children.Add(x);
        }

        #endregion

        #region AddTreeNode

        /// <summary>
        /// Adds a TypeDefinition to the given node
        /// </summary>
        /// <param name="node">Destination node</param>
        /// <param name="type">Type to inject</param>
        /// <param name="afterModule">Action to call when the destination module is available</param>
        /// <param name="afterEnclosingType">Action to call when the enclosing type is available</param>
        public static void AddTreeNode(ILSpyTreeNode node, TypeDefinition type, Action<ModuleDefinition> afterModule, Action<TypeDefinition> afterEnclosingType)
        {
            //Ensures the lazy children of the node
            node.EnsureLazyChildren();

            //Checks if the node is a module or not
            if (node is ModuleTreeNode)
            {
                //Module node
                var moduleNode = (ModuleTreeNode)node;

                //Injects in the module
                var module = moduleNode.Module;
                module.Types.Add(type);
                if (afterModule != null)
                    afterModule(module);

                //Checks for the namespace
                var namespaceNode =
                    moduleNode.Children
                    .OfType<NamespaceTreeNode>()
                    .FirstOrDefault(x => x.Text.ToString().ToLower() == (string.IsNullOrEmpty(type.Namespace) ? "-" : type.Namespace.ToLower()));
                if (namespaceNode != null)
                {
                    //Adds the node to the namespace
                    namespaceNode.Children.Add(new ILEditTreeNode(type, false));
                    TreeHelper.SortChildren(namespaceNode);
                }
                else
                {
                    //Creates a new namespace containing the new type and adds it to the module node
                    namespaceNode = new NamespaceTreeNode(type.Namespace);
                    namespaceNode.Children.Add(new ILEditTreeNode(type, false));
                    moduleNode.Children.Add(namespaceNode);
                    TreeHelper.SortChildren(moduleNode);
                }
            }
            else
            {
                //Marks the class as nested public
                type.Attributes |= TypeAttributes.NestedPublic;
                type.IsNestedPublic = true;

                //Injects in the type
                var type2 = (TypeDefinition)((IMemberTreeNode)node).Member;
                type2.NestedTypes.Add(type);
                if (afterEnclosingType != null)
                    afterEnclosingType(type2);

                //Adds a node to the tree
                node.Children.Add(new ILEditTreeNode(type, false));
                TreeHelper.SortChildren((TypeTreeNode)node);
            }
        }

        #endregion
    }
}

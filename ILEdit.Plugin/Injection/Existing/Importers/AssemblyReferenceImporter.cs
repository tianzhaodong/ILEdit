﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using ICSharpCode.TreeView;
using ICSharpCode.ILSpy.TreeNodes;

namespace ILEdit.Injection.Existing.Importers
{
    internal class AssemblyReferenceImporter : MemberImporter, IMetadataTokenProvider
    {
        public AssemblyReferenceImporter(IMetadataTokenProvider member, MemberImportingSession session)
            : base(member, session.DestinationModule, session)
        {
        }

        protected override bool CanImportCore(Mono.Cecil.IMetadataTokenProvider member, Mono.Cecil.IMetadataTokenProvider destination)
        {
            return member is AssemblyNameReference && destination is ModuleDefinition;
        }

        protected override void ScanCore(MemberImportingOptions options, List<MemberImporter> importList)
        {
            //Checks that the task hasn't been canceled
            options.CancellationToken.ThrowIfCancellationRequested();
        }

        protected override IEnumerable<IMetadataTokenProvider> GetMembersForPreview()
        {
            return new IMetadataTokenProvider[] { this };
        }

        protected override IMetadataTokenProvider ImportCore(MemberImportingOptions options, SharpTreeNode node)
        {
            //Checks that the task hasn't been canceled
            options.CancellationToken.ThrowIfCancellationRequested();

            //Assembly and module
            var asm = (AssemblyNameReference)Member;
            var module = ((ModuleDefinition)Destination);

            //Adds the reference only if it doesn't already exist
            module.AssemblyReferences.Add(asm);
            Helpers.Tree.GetModuleNode(module)
                .Children.EnsureLazyChildren().FirstOrDefault(x => x is ReferenceFolderTreeNode)
                .AddChildAndColorAncestors(new ILEditTreeNode(asm, true));

            //Returns null
            return null;
        }

        public MetadataToken MetadataToken
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}

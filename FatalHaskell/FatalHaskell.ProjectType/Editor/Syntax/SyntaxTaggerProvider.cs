using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.ProjectSystem;
using FatalHaskell.External;
using Bearded.Monads;
using System.Windows;

namespace FatalHaskell.Editor
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("hs")]
    [AppliesTo("FatalHaskell")]
    [TagType(typeof(ClassificationTag))]
    class SyntaxTaggerProvider : ITaggerProvider
    {

        [Import]
        internal IClassificationTypeRegistryService registry { get; set; }

        [Import]
        ITextDocumentFactoryService documentFactoryService;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITextDocument curDoc = null;
            String fileName = null;
            if (documentFactoryService.TryGetTextDocument(buffer, out curDoc))
            {
                fileName = curDoc.FilePath;
            }


            return FHIntero.Instance(fileName).Unify(
                intero =>
                {
                    return new SyntaxTagger(buffer, intero, registry) as ITagger<T>;
                },
                error =>
                {
                    MessageBox.Show(error.ToString());
                    return null;
                }
                );
        }
    }
}

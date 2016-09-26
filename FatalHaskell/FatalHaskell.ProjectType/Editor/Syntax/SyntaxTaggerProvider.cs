using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.ProjectSystem;

namespace FatalHaskell.Editor
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("hs")]
    [AppliesTo("FatalHaskell")]
    [TagType(typeof(ClassificationTag))]
    class SyntaxTaggerProvider : ITaggerProvider
    {

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new SyntaxTagger(buffer, ClassificationTypeRegistryService) as ITagger<T>;
        }
    }
}

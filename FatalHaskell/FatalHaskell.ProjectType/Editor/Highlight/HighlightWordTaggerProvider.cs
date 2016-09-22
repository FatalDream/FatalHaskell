﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.Windows.Media;
using Microsoft.VisualStudio.ProjectSystem;

namespace FatalHaskell.Editor.Highlight
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("hs")]
    [TagType(typeof(ErrorTag))]
    [AppliesTo("FatalHaskell")]
    internal class HighlightWordTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal ITextSearchService TextSearchService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }


        [Import]
        ITextDocumentFactoryService documentFactoryService;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            //provide highlighting only on the top buffer 
            if (textView.TextBuffer != buffer)
                return null;

            ITextStructureNavigator textStructureNavigator =
                TextStructureNavigatorSelector.GetTextStructureNavigator(buffer);

            ITextDocument curDoc = null;
            String fileName = null;
            if (documentFactoryService.TryGetTextDocument(buffer, out curDoc))
            {
                fileName = curDoc.FilePath;
            }

            return new HighlightWordTagger(textView, buffer, TextSearchService, textStructureNavigator, fileName) as ITagger<T>;
        }
    }
}

using System;
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
using System.IO;

namespace FatalHaskell.Editor
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("hs")]
    [TagType(typeof(ErrorTag))]
    [AppliesTo("FatalHaskell")]
    internal sealed class ErrorTaggerProvider : IViewTaggerProvider
    {

        [Import]
        private IBufferTagAggregatorFactoryService _aggregatorFactory = null;

        [Import]
        ITextDocumentFactoryService documentFactoryService;

        //[ImportingConstructor]
        //ErrorTaggerProvider(UnconfiguredProject project)
        //{
        //    String full = project.FullPath;
        //    projectPath = Path.GetDirectoryName(full);
        //}

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        private String projectPath;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            ITextDocument curDoc = null;
            String fileName = null;
            if (documentFactoryService.TryGetTextDocument(buffer, out curDoc))
            {
                fileName = curDoc.FilePath;
            }

            ITagAggregator<FRErrorTag> asmTagAggregator = _aggregatorFactory.CreateTagAggregator<FRErrorTag>(buffer);
            return new ErrorTagger(textView, buffer, fileName, asmTagAggregator, this) as ITagger<T>;
        }
    }
}

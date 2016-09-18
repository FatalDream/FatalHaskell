using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FatalHaskell.Editor
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("FH Completion Controller")]
    [ContentType("hs")]
    [AppliesTo("FatalHaskell")]
    class FHCompletionControllerProvider : IIntellisenseControllerProvider
    {

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new FHCompletionController(textView, subjectBuffers, this);
        }
    }
}

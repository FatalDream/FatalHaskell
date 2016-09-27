using FatalHaskell.External;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalHaskell.Editor
{
    class SyntaxTokenFactory
    {
        private IClassificationTypeRegistryService registry;
        private FHIntero intero;

        public SyntaxTokenFactory(FHIntero intero, IClassificationTypeRegistryService registry)
        {
            this.intero = intero;
            this.registry = registry;
        }

        public SyntaxToken Create(SnapshotSpan span)
        {
            String text = span.GetText();
            SyntaxTokenKind kind;

            if (Char.IsUpper(text.First()))
                kind = SyntaxTokenKind.Type;

            else if (text == "::" || text == "->")
                kind = SyntaxTokenKind.TypeSymbol;

            else if (intero.GetKeywords().Contains(text))
                kind = SyntaxTokenKind.Keyword;

            else
                kind = SyntaxTokenKind.Other;

            return new SyntaxToken(span, kind, registry);
        }
    }
}

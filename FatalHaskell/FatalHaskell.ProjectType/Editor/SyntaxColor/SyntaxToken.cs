using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalHaskell.Editor
{
    enum SyntaxTokenKind
    {
        Type,
        TypeSymbol,
        Keyword,
        Other
    }

    class SyntaxToken
    {
        public SyntaxToken(SnapshotSpan span, SyntaxTokenKind kind, IClassificationTypeRegistryService registry)
        {
            this.span = span;
            this.kind = kind;
            this.registry = registry;
        }

        private SnapshotSpan span;
        private SyntaxTokenKind kind;
        private IClassificationTypeRegistryService registry;
        

        public TagSpan<ClassificationTag> GetTagSpan()
        {
            String classification;

            switch (kind)
            {
                case SyntaxTokenKind.Type:
                    classification = "haskell.type";
                    break;
                case SyntaxTokenKind.TypeSymbol:
                    classification = "haskell.typesymbol";
                    break;
                case SyntaxTokenKind.Keyword:
                    classification = "keyword";
                    break;
                default:
                    classification = "text";
                    break;
            }

            return new TagSpan<ClassificationTag>(span,
                new ClassificationTag(registry.GetClassificationType(classification)));
        }


    }
}

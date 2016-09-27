using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.Linq;
using FatalHaskell.External;

namespace FatalHaskell.Editor
{
    class SyntaxTagger : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        SyntaxTokenFactory tokenFactory;

        internal SyntaxTagger(ITextBuffer buffer, Intero intero, IClassificationTypeRegistryService registry)
        {
            _buffer = buffer;
            tokenFactory = new SyntaxTokenFactory(intero, registry);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (SnapshotSpan curSpan in spans)
            {
                var start = curSpan.Start;
                var end = curSpan.End;
                

                while (start != end)
                {
                    var startWord = SkipWhile(start, end, Char.IsWhiteSpace);
                    var endWord = SkipWhile(startWord, end, c => !Char.IsWhiteSpace(c));
                    start = endWord;

                    if (startWord != endWord)
                    {
                        var wordSpan = new SnapshotSpan(startWord, endWord);
                        yield return tokenFactory.Create(wordSpan).GetTagSpan();
                    }
                }
                
            }
            
        }

        private static SnapshotPoint SkipWhile(SnapshotPoint p, SnapshotPoint end, Func<char, bool> f)
        {
            SnapshotPoint t = p;

            while (t != end && f(t.GetChar()))
            {
                t += 1;
            }
            return t;
        }
    }
}

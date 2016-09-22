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
using FatalHaskell.External;
using Bearded.Monads;
using Microsoft.VisualStudio.Text.Adornments;

namespace FatalHaskell.Editor
{

    internal sealed class ErrorTagger : ITagger<ErrorTag>
    {
        private readonly ITextView _view;
        private readonly ITextBuffer _sourceBuffer;
        private readonly ITagAggregator<ErrorTag> _aggregator;
        private ErrorTaggerProvider _provider;
        private readonly String _filename;
        private ErrorContainer _errorContainer;

        private List<ErrorSnapshotSpan> _errorSpans;

        internal ErrorTagger(
                ITextView view,
                ITextBuffer buffer,
                String filename,
                ITagAggregator<ErrorTag> asmTagAggregator,
                ErrorTaggerProvider provider)
        {
            this._view = view;
            this._sourceBuffer = buffer;
            this._aggregator = asmTagAggregator;
            this._provider = provider;
            this._view.LayoutChanged += ViewLayoutChanged;
            _filename = filename;
            _errorSpans = new List<ErrorSnapshotSpan>();
            FHIntero.Instance(filename).WhenSuccess(i => i.ErrorsChanged += On_ErrorsChanged);
        }

        private void On_ErrorsChanged(ErrorContainer obj)
        {
            var spanList = obj.Find()
                .Select(e =>
                {
                    SnapshotPoint start = _sourceBuffer.CurrentSnapshot.Lines.ElementAt(e.line - 1).Start.Add(e.column - 1);
                    ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_sourceBuffer);
                    TextExtent extent = navigator.GetExtentOfWord(start);
                    SnapshotSpan span = new SnapshotSpan(start, start.Add(3));
                    return new ErrorSnapshotSpan(span, String.Join("\n", e.messages));
                }).ToList();

            _errorSpans = spanList;

            foreach (var span in spanList)
            {
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span.Span));
            }

            
        }

        void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (var span in e.NewOrReformattedSpans)
            {
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        IEnumerable<ITagSpan<ErrorTag>> ITagger<ErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            //foreach (IMappingTagSpan<FRErrorTag> tagSpan in _aggregator.GetTags(spans))
            //{
            //    NormalizedSnapshotSpanCollection tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
            //    if (tagSpans[0].Contains(10))
            //    { // test whether tagSpan has an error here
            //        yield return new TagSpan<ErrorTag>(tagSpans[0], new ErrorTag("syntax error", "my error message"));
            //    }
            //}
            
            List<ErrorSnapshotSpan> errorSpans = _errorSpans;

            if (spans.Count == 0 || errorSpans.Count == 0)
                yield break;


            // If the requested snapshot isn't the same as the one our words are on, translate our spans to the expected snapshot 
            

            // THIS
            if (spans[0].Snapshot != errorSpans[0].start.Snapshot)
            {
                errorSpans = new List<ErrorSnapshotSpan>(
                    errorSpans.Select(errSpan => errSpan.MapSpan(span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeInclusive))));

                //currentWord = currentWord.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive);
            }
            // END THIS

            //foreach (SnapshotSpan span in NormalizedSnapshotSpanCollection.Overlap(errorSpans, spans))
            //{
            //    yield return new TagSpan<ErrorTag>(span, new ErrorTag("syntax error", "what?"));
            //}

            foreach (var errSpan in errorSpans)
            {
                foreach (var span in spans)
                {
                    if (errSpan.start.IntersectsWith(span))
                    {
                        yield return new TagSpan<ErrorTag>(errSpan.start, new ErrorTag(PredefinedErrorTypeNames.SyntaxError, errSpan.Message));
                    }
                }
            }

            //foreach (var span in spans)
            //{
            //    yield return new TagSpan<ErrorTag>(span, new ErrorTag("syntax error", "what?"));
            //}

            //if (IndexR.Get().IsSome)
            //{
            //    IndexR index = IndexR.Get().ForceValue();
            //    foreach (var span in spans)
            //    {
            //        RustcError err = _errorContainer.TryFind();
            //        if (err != null)
            //        {
            //            yield return new TagSpan<ErrorTag>(span, new ErrorTag("syntax error", "what?"));
            //        }
            //    }
            //}
        }
    }
}

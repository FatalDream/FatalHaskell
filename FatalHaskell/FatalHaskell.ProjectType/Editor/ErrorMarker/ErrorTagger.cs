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
using FatalHaskell.External;
using Bearded.Monads;
using Microsoft.VisualStudio.Text.Adornments;
using FatalIDE.Core;

namespace FatalHaskell.Editor
{

    internal sealed class ErrorTagger : ITagger<ErrorTag>
    {
        private readonly ITextView _view;
        private readonly ITextBuffer _sourceBuffer;
        private readonly ITagAggregator<ErrorTag> _aggregator;
        private ErrorTaggerProvider _provider;
        private readonly String relativeFilename;
        private ErrorContainer _errorContainer;
        private Intero intero;
        

        internal ErrorTagger(
                ITextView view,
                ITextBuffer buffer,
                String filename,
                ITagAggregator<ErrorTag> asmTagAggregator,
                ErrorTaggerProvider provider,
                Intero intero)
        {
            this._view = view;
            this._sourceBuffer = buffer;
            this._aggregator = asmTagAggregator;
            this._provider = provider;
            this._view.LayoutChanged += ViewLayoutChanged;
            this.intero = intero;
            intero.ErrorsChanged += On_ErrorsChanged;

            String projectDir = ProjectTree.FindProjectDir(filename);
            relativeFilename = ProjectTree.GetPathDiff(projectDir, filename)
                .Unify(
                    pathDiff => pathDiff,
                    ()       => "" );
        }

        private void On_ErrorsChanged(ErrorContainer obj)
        {

            _errorContainer = obj;
            
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_sourceBuffer.CurrentSnapshot, 0, _sourceBuffer.CurrentSnapshot.Length)));
        }

        void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            intero.UpdateFile(relativeFilename, e.NewSnapshot.GetText());
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        IEnumerable<ITagSpan<ErrorTag>> ITagger<ErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            

            List<InteroError> errors = _errorContainer?.Find(relativeFilename) ?? new List<InteroError>();

            if (spans.Count == 0 || errors.Count == 0)
                yield break;

            foreach (var error in errors)
            {
                foreach (var span in spans)
                {
                    //SnapshotPoint start = _sourceBuffer.CurrentSnapshot.Lines.ElementAt(error.line - 1).Start.Add(error.colStart - 1);
                    //ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_sourceBuffer);
                    //SnapshotSpan errorSpan = navigator.GetExtentOfWord(start).Span;


                    var line = TryIndex(error.line - 1, l => _sourceBuffer.CurrentSnapshot.Lines.ElementAt(l)).Start;
                    SnapshotPoint start = TryIndex(error.colStart - 1, c => line.Add(c));

                    int maxLength = start.Snapshot.Length - start.Position;
                    int suggestedLength = error.colEnd - error.colStart + 1;

                    SnapshotPoint end;
                    if (suggestedLength > maxLength)
                        end = start.Add(maxLength);
                    else
                        end = start.Add(suggestedLength);

                    SnapshotSpan errorSpan = new SnapshotSpan(start, end);

                    if (span.IntersectsWith(errorSpan))
                    {
                        yield return new TagSpan<ErrorTag>(errorSpan, new ErrorTag(PredefinedErrorTypeNames.SyntaxError, error.messages[0]));
                    }
                }
            }
        }

        static private R TryIndex<R>(int start, Func<int,R> f)
        {
            try
            {
                return f(start);
            }
            catch (Exception)
            {
                return TryIndex(start - 1, f);
            }
        }
    }
}

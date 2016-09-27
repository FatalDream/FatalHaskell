using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using FatalHaskell.External;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using FatalIDE.Core;

namespace FatalHaskell.Editor
{
    internal class FHCompletionSource : ICompletionSource
    {
        private FHCompletionSourceProvider m_sourceProvider;
        private ITextBuffer m_textBuffer;
        private List<Completion> m_compList;
        private Intero intero;
        private readonly String relativeFilename;

        public FHCompletionSource(FHCompletionSourceProvider sourceProvider, ITextBuffer textBuffer, Intero intero, String relativeFilename)
        {
            m_sourceProvider = sourceProvider;
            m_textBuffer = textBuffer;
            this.intero = intero;
            this.relativeFilename = relativeFilename;

            textBuffer.Changed += TextBuffer_Changed;
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs args)
        {
            
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {

            var tokenSpan = FindTokenSpanAtPosition(session.GetTriggerPoint(m_textBuffer), session);

            List<String> strList = ThreadHelper.JoinableTaskFactory.Run(() =>
            {
                return intero.GetCompletions(relativeFilename, tokenSpan.GetText(m_textBuffer.CurrentSnapshot));
            });

            //List<string> strList = new List<string>();
            //strList.Add("addition");
            //strList.Add("adaptation");
            //strList.Add("subtraction");
            //strList.Add("summation");
            m_compList = new List<Completion>();
            foreach (string str in strList)
                m_compList.Add(new Completion(str, str, str, null, null));

            completionSets.Add(new CompletionSet(
                "Tokens",    //the non-localized title of the tab
                "Tokens",    //the display title of the tab
                FindTokenSpanAtPosition(session.GetTriggerPoint(m_textBuffer),
                    session),
                m_compList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition);
            currentPoint = currentPoint.TranslateTo(m_textBuffer.CurrentSnapshot, PointTrackingMode.Negative);

            var line = currentPoint.GetContainingLine();
            SnapshotPoint start = currentPoint;

            while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
            {
                start -= 1;
            }

            ITextStructureNavigator navigator = m_sourceProvider.NavigatorService.GetTextStructureNavigator(m_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(start);
            return m_textBuffer.CurrentSnapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}

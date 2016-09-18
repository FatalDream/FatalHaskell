using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FatalHaskell.Editor
{
    class FHCompletionController : IIntellisenseController
    {
        private ITextView m_textView;
        private IList<ITextBuffer> m_subjectBuffers;
        private FHCompletionControllerProvider m_provider;
        private ICompletionSession m_session;

        internal FHCompletionController(ITextView textView, IList<ITextBuffer> subjectBuffers, FHCompletionControllerProvider provider)
        {
            m_textView = textView;
            m_subjectBuffers = subjectBuffers;
            m_provider = provider;

            m_textView.TextBuffer.Changed += OnTextBuffer_Changed;
            //m_textView.MouseHover += this.OnTextViewMouseHover;
        }

        private void OnTextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            ////find the mouse position by mapping down to the subject buffer
            //SnapshotPoint? point = m_textView.BufferGraph.MapDownToFirstMatch
            //     (new SnapshotPoint(m_textView.TextSnapshot, e.After.),
            //    PointTrackingMode.Positive,
            //    snapshot => m_subjectBuffers.Contains(snapshot.TextBuffer),
            //    PositionAffinity.Predecessor);

            //if (point != null)
            //{
            //    ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
            //    PointTrackingMode.Positive);

            //    if (!m_provider.QuickInfoBroker.IsQuickInfoActive(m_textView))
            //    {
            //        m_session = m_provider.QuickInfoBroker.TriggerQuickInfo(m_textView, triggerPoint, true);
            //    }
            //}

            //the caret must be in a non-projection location 
            

            if (!m_provider.CompletionBroker.IsCompletionActive(m_textView))
            {
                SnapshotPoint? caretPoint = m_textView.Caret.Position.Point.GetPoint(
                    textBuffer => !textBuffer.ContentType.IsOfType("projection"),
                    PositionAffinity.Predecessor);
                if (caretPoint == null)
                    return;

                m_session = m_provider.CompletionBroker.CreateCompletionSession(
                    m_textView,
                    caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                    true);
                
                //var session = m_provider.CompletionBroker.CreateCompletionSession(m_textView, point, true);
                m_session.Start();
                m_session.Filter();
            }
            else
            {
                m_session.Filter();
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            
        }

        public void Detach(ITextView textView)
        {
            if (m_textView == textView)
            {
                //m_textView.MouseHover -= this.OnTextViewMouseHover;
                m_textView = null;
            }
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}

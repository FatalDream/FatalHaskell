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
using Bearded.Monads;
using Microsoft.VisualStudio.Text.Adornments;

namespace FatalHaskell.Editor.Visual
{
    class ErrorSnapshotSpan
    {
        public SnapshotSpan Span;
        public String Message;

        public ErrorSnapshotSpan(SnapshotSpan span, String message)
        {
            this.Span = span;
            this.Message = message;
        }

        public ErrorSnapshotSpan MapSpan(Func<SnapshotSpan,SnapshotSpan> f)
        {
            return new ErrorSnapshotSpan(f(Span), Message);
        }
    }
}

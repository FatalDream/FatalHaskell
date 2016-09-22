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

namespace FatalHaskell.Editor
{
    class ErrorSnapshotSpan
    {
        public SnapshotPoint start;
        public String Message;

        public ErrorSnapshotSpan(SnapshotPoint start, String message)
        {
            this.start = start;
            this.Message = message;
        }

        public ErrorSnapshotSpan MapSpan(Func<SnapshotPoint,SnapshotPoint> f)
        {
            return new ErrorSnapshotSpan(f(start), Message);
        }
    }
}

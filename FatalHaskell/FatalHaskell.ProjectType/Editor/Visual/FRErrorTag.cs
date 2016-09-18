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
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.TextManager.Interop;

namespace FatalHaskell.Editor
{
    class FRErrorTag : ErrorTag
    {
        public FRErrorTag() : base("MarkerFormatDefinition/ErrorFormatDefinition") { }
    }


    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/ErrorFormatDefinition")]
    [UserVisible(true)]
    [AppliesTo("FatalHaskell")]
    internal class ErrorFormatDefinition : MarkerFormatDefinition
    {
        public ErrorFormatDefinition()
        {
            this.BackgroundColor = Colors.OrangeRed;
            this.ForegroundColor = Colors.Transparent;
            this.DisplayName = "Error";
            this.ZOrder = 10;
        }
    }
}

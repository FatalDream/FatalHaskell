using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FatalHaskell.Editor
{

    internal static class ClassificationTypes
    {
        [Export]
        [Name("haskell.type")]
        [BaseDefinition("text")]
        [AppliesTo("FatalHaskell")]
        internal static ClassificationTypeDefinition HaskellTypeDefinition;

        [Export]
        [Name("haskell.typesymbol")]
        [BaseDefinition("text")]
        [AppliesTo("FatalHaskell")]
        internal static ClassificationTypeDefinition HaskellTypeSymbolDefinition;
    }


    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "haskell.type")]
    [Name("type")]
    [DisplayName("Type")]
    [AppliesTo("FatalHaskell")]
    [UserVisible(true)]
    [Microsoft.VisualStudio.Utilities.Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class TypeClassificationFormat : ClassificationFormatDefinition
    {
        public TypeClassificationFormat()
        {
            ForegroundColor = Color.FromRgb(43, 145, 175);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "haskell.typesymbol")]
    [Name("typesymbol")]
    [DisplayName("TypeSymbol")]
    [AppliesTo("FatalHaskell")]
    [UserVisible(true)]
    [Microsoft.VisualStudio.Utilities.Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class TypeSymbolClassificationFormat : ClassificationFormatDefinition
    {
        public TypeSymbolClassificationFormat()
        {
            ForegroundColor = Color.FromRgb(0xcd, 0x26, 0x26);
        }
    }
}

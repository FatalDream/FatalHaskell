using Bearded.Monads;
using EnvDTE;
using FatalHaskell.External;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FatalHaskell.Editor
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("hs")]
    [Name("token completion")]
    [AppliesTo("FatalHaskell")]
    internal class FHCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        DTE dte;

        public Option<FHIntero> intero;
        
        [ImportingConstructor]
        FHCompletionSourceProvider(SVsServiceProvider ServiceProvider)
        {

            dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            String mainDir = Path.GetDirectoryName(dte.ActiveDocument.FullName);

            intero = FHIntero.Instance(mainDir).Unify(
                i   => Option<FHIntero>.Return(i),
                err => {
                    MessageBox.Show(err.ToString());
                    return Option<FHIntero>.None; 
                });
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return intero
                .Select(  index => new FHCompletionSource(this, textBuffer, index, dte))
                .Else(    () => (FHCompletionSource)null);
        }


    }
}

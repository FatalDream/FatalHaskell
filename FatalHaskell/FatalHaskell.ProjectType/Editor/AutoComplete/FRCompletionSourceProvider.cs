using Bearded.Monads;
using EnvDTE;
using FatalHaskell.External;
using FatalIDE.Core;
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

        [Import]
        ITextDocumentFactoryService documentFactoryService;
        
        public Option<Intero> intero;
        
        [ImportingConstructor]
        FHCompletionSourceProvider(SVsServiceProvider ServiceProvider)
        {
            DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            String mainDir = Path.GetDirectoryName(dte.ActiveDocument.FullName);

            intero = Intero.Instance(mainDir).Unify(
                i   => Option<Intero>.Return(i),
                err => {
                    MessageBox.Show(err.ToString());
                    return Option<Intero>.None; 
                });
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer buffer)
        {
            ITextDocument curDoc = null;
            Option<String> fileName = Option<String>.None;
            if (documentFactoryService.TryGetTextDocument(buffer, out curDoc))
            {
                fileName = ProjectTree.RelativeFilename(curDoc.FilePath);
            }

            return intero
                .SelectMany( i  => fileName.Select( f => new FHCompletionSource(this, buffer, i, f)))
                .Else(   () => null);
        }


    }
}

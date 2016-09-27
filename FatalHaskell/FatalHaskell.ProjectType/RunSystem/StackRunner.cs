using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;
using FatalHaskell.External;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using System.IO;

namespace FatalHaskell.RunSystem
{
    [Export(typeof(IRunner))]
    [AppliesTo("FatalHaskell")]
    class StackRunner : IRunner
    {
        private String projectPath;

        [ImportingConstructor]
        public StackRunner(UnconfiguredProject project)
        {
            String full = project.FullPath;
            projectPath = Path.GetDirectoryName(full);
        }


        public EitherSuccessOrError<Success, Error<string>> Run()
        {
            return
            HaskellStack.Instance()
                .Select(stack => stack.Run(projectPath))
                .Select(output => new Success());
        }
    }
}

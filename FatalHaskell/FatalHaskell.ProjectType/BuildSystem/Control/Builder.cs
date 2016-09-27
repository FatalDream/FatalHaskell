using FatalHaskell.External;
using Microsoft.VisualStudio.ProjectSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Bearded.Monads;
using FatalIDE.Core;

namespace FatalHaskell.BuildSystem
{

    [Export(typeof(IBuilder))]
    [AppliesTo("FatalHaskell")]

    internal class Builder : IBuilder
    {
        private String projectPath;

        [ImportingConstructor]
        public Builder(UnconfiguredProject project)
        {
            String full = project.FullPath;
            projectPath = Path.GetDirectoryName(full);
        }

        public async Task<EitherSuccessOrError<Success,Error<String>>> Build()
        {
            return await
            HaskellStack.Instance()
                .SelectAsync(stack => stack.Build(projectPath, new OutputPaneLogger()));
        }
    }
}

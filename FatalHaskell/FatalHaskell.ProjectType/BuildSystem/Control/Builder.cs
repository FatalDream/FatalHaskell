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

        public void Build()
        {
            HaskellStack.Instance()
                .WhenSuccess(stack => stack.Build(projectPath, new OutputPaneLogger()));

            MessageBox.Show("Builder called!");
        }
    }
}

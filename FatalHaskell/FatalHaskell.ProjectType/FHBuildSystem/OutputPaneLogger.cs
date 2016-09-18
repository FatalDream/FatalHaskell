using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio;

namespace FatalHaskell.FHBuildSystem
{
    public class OutputPaneLogger : ILogger
    {

        public void WriteMessage(String msg)
        {
            var joinableTaskFactoryInstance = ThreadHelper.JoinableTaskFactory;

            joinableTaskFactoryInstance.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                Guid generalPaneGuid = VSConstants.GUID_OutWindowDebugPane; // P.S. There's also the GUID_OutWindowDebugPane available.
                IVsOutputWindowPane generalPane;
                outWindow.GetPane(ref generalPaneGuid, out generalPane);

                generalPane.OutputString(msg + "\n");
                generalPane.Activate(); // Brings this pane into view
            });
        }
    }
}

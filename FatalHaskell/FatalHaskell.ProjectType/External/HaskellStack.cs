using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;
using FatalHaskell.FHBuildSystem;

namespace FatalHaskell.External
{
    class HaskellStack
    {
        #region Constructor
        private static HaskellStack Singleton = null;

        public static EitherSuccessOrError<HaskellStack, Error<String>> Instance()
        {
            if (Singleton != null)
            {
                return EitherSuccessOrError<HaskellStack, Error<String>>.Create(Singleton);
            }
            else
            {
                return Communication.ReadProcess("stack", "--version")
                    .Select(output => output.Raw())
                    .Select(s => new HaskellStack(s));
            }
        }

        private HaskellStack(String s)
        {

        }
        #endregion Constructor

        #region Commands
        public void Build(String pathToProject, ILogger l)
        {
            Communication.StartProcess("stack", "build", pathToProject, s => l.WriteMessage(s), err => l.WriteMessage(err), () => { return; });
        }

        #endregion Commands

    }
}

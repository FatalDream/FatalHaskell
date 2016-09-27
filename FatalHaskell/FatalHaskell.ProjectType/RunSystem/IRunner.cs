using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;

namespace FatalHaskell.RunSystem
{
    interface IRunner
    {
        EitherSuccessOrError<Success,Error<String>> Run();
    }
}

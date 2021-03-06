﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;

namespace FatalHaskell.BuildSystem
{
    interface IBuilder
    {
        Task<EitherSuccessOrError<Success,Error<String>>> Build();
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalHaskell.FHBuildSystem
{
    interface ILogger
    {
        void WriteMessage(String msg);
    }
}
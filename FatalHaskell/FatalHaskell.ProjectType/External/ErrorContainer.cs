using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalHaskell.External
{
    class ErrorContainer
    {
        public ErrorContainer()
        {
            this.Errors = new List<InteroError>();
        }

        public List<InteroError> Find()
        {
            return Errors;
        }

        public List<InteroError> Errors;
    }
}

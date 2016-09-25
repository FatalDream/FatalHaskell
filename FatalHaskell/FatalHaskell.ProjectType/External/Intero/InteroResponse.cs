using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalHaskell.External
{
    class InteroResponse
    {
        public List<String> output;
        public List<String> error;

        public InteroResponse(List<String> output, List<String> error)
        {
            this.output = output;
            this.error = error;
        }

        public void Clear()
        {
            output.Clear();
            error.Clear();
        }
    }
}

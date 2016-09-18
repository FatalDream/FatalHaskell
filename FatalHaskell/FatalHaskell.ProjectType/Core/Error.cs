using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalIDE.Core
{
    public class Error<T>
    {
        public Error(T x) 
        {
            Value = x;
        }

        public override string ToString()
        {
            return "Error: " + Value.ToString();
        }

        public T Value;
    }
}

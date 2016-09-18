using Bearded.Monads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalHaskell.External
{
    class InteroError
    {

        public static Option<InteroError> Create(String s)
        {
            //if (s.StartsWith(HEADER))
            //{
            //    var args = s.Split('\t');
            //    if (args.Length == 4)
            //    {
            //        return new RustcError(int.Parse(args[1]), int.Parse(args[2]), args[3]);
            //    }
            //}
            //return Option<RustcError>.None;

            return new InteroError(s);
        }

        //private InteroError(int PosLow, int PosHigh, String message)
        //{
        //    this.PosLow = PosLow;
        //    this.PosHigh = PosHigh;
        //    this.message = message;
        //}

        private InteroError(String msg)
        {
            this.PosLow = 0;
            this.PosHigh = 20;
            this.message = msg;
        }

        public int PosLow;
        public int PosHigh;
        public String message;
    }
}

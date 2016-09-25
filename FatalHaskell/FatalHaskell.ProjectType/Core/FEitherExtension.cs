using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;

namespace FatalIDE.Core
{
    public static class FEitherExtension
    {
        public static EitherSuccessOrError<Tuple<A,B>,Err> ZipM<A,B,Err>(this EitherSuccessOrError<A,Err> a, EitherSuccessOrError<B,Err> b)
        {
            return a.SelectMany(inner_a => b.Select(inner_b => Tuple.Create(inner_a, inner_b)));
        }
    }
}

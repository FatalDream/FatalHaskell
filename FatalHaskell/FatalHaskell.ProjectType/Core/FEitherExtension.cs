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
        public static EitherSuccessOrError<Tuple<A, B>, Err> ZipM<A, B, Err>(this EitherSuccessOrError<A, Err> a, EitherSuccessOrError<B, Err> b)
        {
            return a.SelectMany(inner_a => b.Select(inner_b => Tuple.Create(inner_a, inner_b)));
        }

        public static Task<EitherSuccessOrError<B, Err>> SelectAsync<A, B, Err>(
            this EitherSuccessOrError<A, Err> self, Func<A, Task<B>> f)
        {
            return self.UnifyAsync<A,EitherSuccessOrError<B, Err>, Err> (
                async a => await f(a),
                b => Task.FromResult(EitherSuccessOrError<B,Err>.Create(b)));
                
        }

        public static Task<B> UnifyAsync<A, B, Err>(
            this EitherSuccessOrError<A, Err> self, Func<A, Task<B>> result, Func<Err, Task<B>> error)
        {
            Task<B> b = Task.FromResult(default(B));

            self.WhenSuccess(a => b = result(a))
                .WhenError(err => b = error(err));

            return b;
        }
    }
}

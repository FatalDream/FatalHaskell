using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;

namespace FatalIDE.Core
{
    public static class FOptionExtension
    {
        public static B Unify<A,B>(this Option<A> self, Func<A,B> someF, Func<B> noneF)
        {
            B result = default(B);
            self
                .WhenSome(a => result = someF(a))
                .WhenNone(() => result = noneF());
            return result;
        }

        public static Option<A> Or<A>(this Option<A> self, Func<Option<A>> f)
        {
            return self.Unify(
                a  => a,
                () => f());
        }

        public static Task<Option<B>> SelectAsync<A,B>(this Option<A> self, Func<A, Task<B>> f)
        {
            return Unify<A, Task<Option<B>>>(
                self,
                async a => await f(a),
                () => Task.FromResult(Option<B>.None));
        }
    }
}

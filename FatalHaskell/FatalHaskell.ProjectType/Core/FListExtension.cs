using System.Collections.Generic;
using Bearded.Monads;
using System;
using System.Linq;

namespace FatalIDE.Core
{
    public static class FListExtension
    {
        public static List<A> ToList<A>(this Option<A> x)
        {
            List<A> result = new List<A>();
            x.WhenSome(a => result.Add(a));
            return result;
        }

        public static List<A> EditLast<A>(this List<A> l, Func<A,A> f, Func<A> n)
        {
            int last = l.Count - 1;
            if (last >= 0)
            {
                l[last] = f(l[last]);
            }
            else
            {
                l.Add(n());
            }
            return l;
        }

        public static IEnumerable<B> FilterMap<A,B>(this IEnumerable<A> xs, Func<A,Option<B>> f)
        {
            return xs.SelectMany(x => f(x).ToList());
        }
    }
}

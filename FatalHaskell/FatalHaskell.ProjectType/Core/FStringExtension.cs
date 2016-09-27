using Bearded.Monads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FatalIDE.Core
{
    public static class FStringExtension
    {
        public static Option<Match> MatchRegex(this String input, String r)
        {
            Match m = Regex.Match(input, r);
            if (m.Success)
                return m;
            else
                return Option<Match>.None;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Bearded.Monads;

namespace FatalIDE.Core
{
    public class ProcessOutput
    {
        public ProcessOutput(String raw)
        {
            this.raw = raw;
        }

        public EitherSuccessOrError<Match, Error<String>> ParseRegex(Regex r)
        {
            Match m = r.Match(raw);
            if (m.Success)
            {
                return EitherSuccessOrError<Match, Error<String>>.Create(m);
            }
            else
            {
                return EitherSuccessOrError<Match, Error<String>>.Create(
                    new Error<String>("Regex '" + r + "' did not match when parsing '" + raw + "'"));
            }
        }

        public String Raw() => raw;

        private String raw;
    }
}

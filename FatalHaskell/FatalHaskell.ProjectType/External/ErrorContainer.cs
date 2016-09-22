using System;
using System.Collections.Generic;
using System.Linq;
using FatalIDE.Core;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;

namespace FatalHaskell.External
{
    class ErrorContainer
    {

        public ErrorContainer()
        {
            errors = new List<InteroError>();
        }

        public void AppendOrCreate(String line)
        {
            int last = errors.Count - 1;
            if (last >= 0)
            {
                errors = errors.SelectMany<InteroError, InteroError>((e, i) =>
                {
                    if (i == last)
                        return e.AppendOrCreate(line);
                    else
                        return new InteroError[] { e };
                })
                .ToList();
            }
            else
            {
                errors = InteroError.Create(line).ToList();
            }
        }



        public List<InteroError> Find()
        {
            return errors;
        }

        public void Clear()
        {
            errors.Clear();
        }

        private List<InteroError> errors;
    }

    
}

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

        public ErrorContainer(String basePath)
        {
            errors = new List<InteroError>();
            this.basePath = basePath;
        }

        public void AppendOrCreate(String line)
        {
            int last = errors.Count - 1;
            if (last >= 0)
            {
                errors = errors.SelectMany<InteroError, InteroError>((e, i) =>
                {
                    if (i == last)
                        return e.AppendOrCreate(line, basePath);
                    else
                        return new InteroError[] { e };
                })
                .ToList();
            }
            else
            {
                errors = InteroError.Create(line, basePath).ToList();
            }
            ErrorsChanged?.Invoke(this);
        }



        public List<InteroError> Find(String relativePath)
        {
            return errors.Where(e => e.path == relativePath).ToList();
        }

        public void Clear()
        {
            errors.Clear();
        }

        public event Action<ErrorContainer> ErrorsChanged;

        private List<InteroError> errors;
        private String basePath;
    }

    
}

using Bearded.Monads;
using FatalIDE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FatalHaskell.External
{
    class InteroError
    {

        public static Option<InteroError> Create(String s)
        {
            Match m = Regex.Match(s, "(.+?):([0-9]+):([0-9]+): (error):");

            if (m.Success)
            {
                String path = m.Groups[1].Value;
                int line = int.Parse(m.Groups[2].Value);
                int column = int.Parse(m.Groups[3].Value);
                return new InteroError(path, line, column);
            }
            else
            {
                return Option<InteroError>.None;
            }
        }

        public List<InteroError> AppendOrCreate(String line)
        {
            String trimmedLine = line.TrimStart(' ', '*');
            int newIndentation = line.Length - trimmedLine.Length;
            if (newIndentation > 0)
            {
                if (newIndentation > currentIndentation)
                    messages.EditLast(last => String.Concat(last, trimmedLine),
                                      () => trimmedLine);
                else
                    messages.Add(line);
                currentIndentation = newIndentation;

                return this.AsOption().ToList();
            }
            else
            {
                List<InteroError> result = Create(line).ToList();
                result.Add(this);
                return result;
            }
        }

        //private InteroError(int PosLow, int PosHigh, String message)
        //{
        //    this.PosLow = PosLow;
        //    this.PosHigh = PosHigh;
        //    this.message = message;
        //}

        private InteroError(String path, int line, int column)
        {
            this.path = path;
            this.line = line;
            this.column = column;
            this.currentIndentation = 0;
            this.messages = new List<String>();
        }

        // properties
        private int currentIndentation;

        // first line
        public int line;
        public int column;
        public String path;

        // other messages
        public List<String> messages;
    }
}

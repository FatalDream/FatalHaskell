﻿using Bearded.Monads;
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

        public static Option<InteroError> Create(String s, String projectPath)
        {
            //Match mCol = Regex.Match(s, "(.+?):([0-9]+):([0-9]+)-([0-9]+): (error):");

            Func<Option<InteroError>> matchSingleCol =
                () => s.MatchRegex("(.+?):([0-9]+):([0-9]+): (error):")
                .SelectMany(singleCol =>
                {
                    int line = int.Parse(singleCol.Groups[2].Value);
                    int col = int.Parse(singleCol.Groups[3].Value);

                    return from path in ProjectTree.GetPathDiff(projectPath, singleCol.Groups[1].Value)
                           select new InteroError(path, line, col, col);
                });


            Func<Option<InteroError>> matchMultiCol =
                () => s.MatchRegex("(.+?):([0-9]+):([0-9]+)-([0-9]+): (error):")
                .SelectMany(multiCol =>
                {
                    int line     = int.Parse(multiCol.Groups[2].Value);
                    int colStart = int.Parse(multiCol.Groups[3].Value);
                    int colEnd   = int.Parse(multiCol.Groups[4].Value);

                    return from path in ProjectTree.GetPathDiff(projectPath, multiCol.Groups[1].Value)
                           select new InteroError(path, line, colStart, colEnd);
                });

            return matchMultiCol() .Or( matchSingleCol);

        }

        

        public List<InteroError> AppendOrCreate(String line, String projectPath)
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
                List<InteroError> result = new List<InteroError>();
                result.Add(this);
                result.AddRange(Create(line, projectPath).ToList());
                return result;
            }
        }

        private InteroError(String path, int line, int colStart, int colEnd)
        {
            this.path = path;
            this.line = line;
            this.colStart = colStart;
            this.colEnd = colEnd;
            this.currentIndentation = 0;
            this.messages = new List<String>();
        }

        // properties
        private int currentIndentation;

        // first line
        public int line;
        public int colStart;
        public int colEnd;
        public String path;

        // other messages
        public List<String> messages;
    }
}

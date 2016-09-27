using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;

namespace FatalHaskell.External
{
    enum StackTargetType
    {
        Lib,
        Exe,
        Test
    }

    class StackTarget
    {
        public readonly Option<String> name;
        public readonly StackTargetType type;

        public static Option<StackTarget> FromStackOutput(String output)
        {
            return
            output.MatchRegex("(.+):(lib|exe|test):(.+)")
                .SelectMany(m =>
                {
                    return from type in TypeFromString(m.Groups[2].Value)
                           select new StackTarget(type, m.Groups[3].Value);
                })

                .Or(() =>

            output.MatchRegex("(.+):(lib|exe|test)")
                .SelectMany(m =>
                {
                    return from type in TypeFromString(m.Groups[2].Value)
                           select new StackTarget(type, Option<String>.None);
                }));
        }

        public StackTarget(StackTargetType type, Option<String> name)
        {
            this.type = type;
            this.name = name;
        }

        static private Option<StackTargetType> TypeFromString(String s)
        {
            switch (s)
            {
                case "lib":
                    return StackTargetType.Lib;
                case "exe":
                    return StackTargetType.Exe;
                case "test":
                    return StackTargetType.Test;
                default:
                    return Option<StackTargetType>.None;
            }
        }
    }
}

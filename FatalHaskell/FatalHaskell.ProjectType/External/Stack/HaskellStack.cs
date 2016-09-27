using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;
using FatalHaskell.BuildSystem;
using System.Windows;

namespace FatalHaskell.External
{
    class HaskellStack
    {
        #region Constructor
        private static HaskellStack Singleton = null;

        public static EitherSuccessOrError<HaskellStack, Error<String>> Instance()
        {
            if (Singleton != null)
            {
                return EitherSuccessOrError<HaskellStack, Error<String>>.Create(Singleton);
            }
            else
            {
                return Communication.ReadProcess("stack", "--version")
                    .Select(output => output.Raw())
                    .Select(s => new HaskellStack(s));
            }
        }

        private HaskellStack(String s)
        {

        }
        #endregion Constructor

        #region Commands
        public async Task<Success> Build(String pathToProject, ILogger l)
        {
            var completed = new TaskCompletionSource<Success>();

            var res = Communication.StartProcess("stack", "build", pathToProject, s => l.WriteMessage(s), err => l.WriteMessage(err), () => { completed.SetResult(new Success()); });

            res.WhenError(err => MessageBox.Show(err.ToString()));

            await completed.Task;

            return new Success();
        }

        public EitherSuccessOrError<ProcessOutput,Error<String>> Run(String pathToProject)
        {
            return
            from targets in GetTargets(pathToProject)
            from target in ChooseExe(targets)
            from result in Communication.ReadProcess("stack", "exec " + target.name, pathToProject)
            select result;
        }

        public EitherSuccessOrError<IEnumerable<StackTarget>,Error<String>> GetTargets(String pathToProject)
        {
            return
            from process in Communication.StartNewProcess("stack", "ide targets", pathToProject)
            select new ProcessOutput(process.StandardError.ReadToEnd())
                            .Lines().FilterMap(StackTarget.FromStackOutput);
        }

        private EitherSuccessOrError<StackTarget,Error<String>> ChooseExe(IEnumerable<StackTarget> targets)
        {
            var applicable = targets.Where(target => target.type == StackTargetType.Exe)
                                    .Where(target => target.name.IsSome);

            if (applicable.Count() < 1)
                return new Error<String>("No executable, named stack target found.");
            else if (applicable.Count() > 1)
                return new Error<String>("There is more than one executable, named stack target.");
            else
                return applicable.First();
        }

        #endregion Commands

    }
}

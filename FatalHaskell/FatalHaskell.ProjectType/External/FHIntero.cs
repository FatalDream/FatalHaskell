using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;
using System.IO;
using System.Windows;

namespace FatalHaskell.External
{
    class FHIntero
    {
        #region Constructor
        private static FHIntero Singleton = null;

        public static EitherSuccessOrError<FHIntero, Error<String>> Instance(String projectDir)
        {
            if (Singleton == null)
            {
                FHIntero intero = new FHIntero();

                return Communication.StartProcess(
                    "stack", "ghci --with-ghc intero",
                    ProjectTree.GetDirWithFile(projectDir, "stack.yaml"),
                    intero.OnReceiveResponse,
                    intero.OnReceiveError)
                    .Select(writer => intero.Initialize(writer))

                    .WhenSuccess(index => Singleton = index)
                    .WhenError(err => MessageBox.Show(err.ToString()));
            }
            else
            {
                return EitherSuccessOrError<FHIntero, Error<String>>.Create(Singleton);
            }
        }

        private FHIntero() { }
        #endregion Constructor



        public event Action<ErrorContainer> ErrorsChanged;

        private Action<String> CommandWriter;

        private TaskCompletionSource<List<String>> ResponseSource;

        private List<String> PartialResponses;

        private ErrorContainer RustcErrors;

        private FHIntero Initialize(Action<String> CommandWriter)
        {
            this.CommandWriter = CommandWriter;
            this.ResponseSource = null;
            this.PartialResponses = new List<String>();
            this.RustcErrors = new ErrorContainer();
            return this;
        }

        public async Task<List<String>> GetCompletions()
        {
            ResponseSource = new TaskCompletionSource<List<String>>();
            CommandWriter("get");
            return await ResponseSource.Task;
        }

        public async Task<List<String>> UpdateAndGetCompletions(String filename, String contents)
        {
            RustcErrors.Errors.Clear();

            ResponseSource = new TaskCompletionSource<List<String>>();
            CommandWriter("update");
            CommandWriter(filename);
            CommandWriter(contents);
            CommandWriter((char)26 + "");
            await ResponseSource.Task;

            ErrorsChanged?.Invoke(RustcErrors);
            return await GetCompletions();
        }


        private void OnReceiveResponse(String response)
        {
            if (response.Contains(">"))
            {
                ResponseSource?.TrySetResult(PartialResponses);
                PartialResponses = new List<String>();
            }
            else
            {
                PartialResponses.Add(response);
            }
        }

        private void OnReceiveError(String error)
        {
            InteroError.Create(error)
                .WhenSome(e => RustcErrors.Errors.Add(e));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;
using System.IO;
using System.Windows;
using System.Diagnostics;

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

                return StartInternal(intero, projectDir)
                    .WhenSuccess(index => Singleton = index)
                    .WhenError(err => MessageBox.Show(err.ToString()));
            }
            else
            {
                return EitherSuccessOrError<FHIntero, Error<String>>.Create(Singleton);
            }
        }

        private static EitherSuccessOrError<FHIntero,Error<String>> StartInternal(FHIntero intero, String projectDir)
        {
            return Communication.StartNewProcess(
                   "stack", "ghci --with-ghc intero",
                   ProjectTree.GetDirWithFile(projectDir, "stack.yaml"))
                 .WhenSuccess(p =>
                 {
                     p.ErrorDataReceived += (e, s) => intero.OnReceiveError(s.Data);
                     p.BeginErrorReadLine();
                     p.StandardInput.WriteLine(":set prompt >");
                     ReadAll(p);
                 })
                 .Select(p => intero.Initialize(p));
        }

        private FHIntero()
        {
            this.Errors = new ErrorContainer();
        }
        #endregion Constructor



        public event Action<ErrorContainer> ErrorsChanged;

        private ErrorContainer Errors;

        private Process CurrentProcess;

        private FHIntero Initialize(Process CurrentProcess)
        {
            this.CurrentProcess = CurrentProcess;
            return this;
        }

        //public async Task<List<String>> GetCompletions()
        //{
        //    ResponseSource = new TaskCompletionSource<List<String>>();
        //    CurrentProcess.StandardInput.WriteLine("get");
        //    return await ResponseSource.Task;
        //}

        public List<String> UpdateAndGetCompletions(String filename, String contents)
        {
            return GetResponse(":r");
        }

        private List<String> GetResponse(String request)
        {
            Errors.Clear();
            CurrentProcess.StandardInput.WriteLine(request);
            List<String> response = ReadAll();
            ErrorsChanged?.Invoke(Errors);
            return response;
        }

        private List<String> ReadAll()
        {
            return ReadAll(CurrentProcess);
        }

        private static List<String> ReadAll(Process p)
        {
            String result = "";
            while (true)
            {
                char c = (char)p.StandardOutput.Read();
                if (c == '>')
                    break;
                else
                    result += c;
            }
            return result.Split('\n').ToList();
        }


        //private void OnReceiveResponse(String response)
        //{
        //    if (response.Contains("Collecting type info"))
        //    {
        //        ResponseSource?.TrySetResult(PartialResponses);
        //        PartialResponses = new List<String>();
        //    }
        //    else
        //    {
        //        PartialResponses.Add(response);
        //    }
        //}

        private void OnReceiveError(String error)
        {
            Errors.AppendOrCreate(error);
        }
    }
}

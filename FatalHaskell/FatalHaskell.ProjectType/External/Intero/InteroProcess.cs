using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearded.Monads;
using FatalIDE.Core;
using System.Diagnostics;
using System.Timers;
using System.Threading;

namespace FatalHaskell.External
{
    class InteroProcess
    {


        public static EitherSuccessOrError<InteroProcess, Error<String>> StartNew(String projectDir)
        {
            InteroProcess process = new InteroProcess();
            return Communication.StartNewProcess(
                       "stack", "ghci --with-ghc intero --ghci-options -ferror-spans",
                       projectDir)
                     .Select(p =>
                     {
                         process.Initialize(p);
                         p.ErrorDataReceived += (e, s) => process.OnReceiveError(s.Data);
                         p.BeginErrorReadLine();
                         p.StandardInput.WriteLine(":set prompt >");
                         process.ReadAll();
                         process.ReadAll();
                         return process;
                     });
        }

        private InteroProcess()
        {}

        Process currentProcess;

        private void Initialize(Process interoProcess)
        {
            this.currentProcess = interoProcess;
            this.currentErrors = new List<String>();
            this.requestSemaphore = new SemaphoreSlim(1, 1);
        }




        ///////////////////////////////////////////////////////////////////////
        #region Communication
        /////////////////

        List<String> currentErrors;
        SemaphoreSlim requestSemaphore;


        public async Task<InteroResponse> GetResponse(String request)
        {
            InteroResponse result;

            await requestSemaphore.WaitAsync();
            try
            {
                currentErrors.Clear();
                currentProcess.StandardInput.WriteLine(request);
                List<String> output = ReadAll();

                await Task.Delay(TimeSpan.FromMilliseconds(1000));

                result = new InteroResponse(output, currentErrors);
            }
            finally
            {
                requestSemaphore.Release();
            }

            return result;
        }

        private List<String> ReadAll()
        {
            String result = "";
            while (true)
            {
                char c = (char)currentProcess.StandardOutput.Read();
                if (c == '>')
                    break;
                else
                    result += c;
            }
            return result.Split('\n').ToList();
        }

        private void OnReceiveError(String error)
        {
            currentErrors.Add(error);
        }

        #endregion
    }
}

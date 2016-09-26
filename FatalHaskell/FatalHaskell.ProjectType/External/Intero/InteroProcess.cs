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
using System.IO;

namespace FatalHaskell.External
{
    class InteroProcess
    {
        private const String EOT = "hakjsdfhkajsfd";

        public static EitherSuccessOrError<InteroProcess, Error<String>> StartNew(String projectDir)
        {
            InteroProcess process = new InteroProcess(projectDir);
            return Communication.StartNewProcess(
                       "stack", "ghci --with-ghc intero --ghci-options -ferror-spans",
                       projectDir)
                     .Select(p =>
                     {
                         process.Initialize(p);
                         p.ErrorDataReceived += (e, s) => process.OnReceiveError(s.Data);
                         p.BeginErrorReadLine();
                         p.StandardInput.WriteLine(":set prompt  \"" + EOT + "\"");
                         process.ReadAll();
                         return process;
                     });
        }

        private InteroProcess(String projectDir)
        {
            this.projectDir = projectDir;
        }

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
        int curRequest = 0;


        public async Task<InteroResponse> GetResponse(String request)
        {
            InteroResponse result;

            await requestSemaphore.WaitAsync();
            try
            {
                curRequest++;
                LogLine("<<<< ", request);

                currentErrors.Clear();
                currentProcess.StandardInput.WriteLine(request);
                List<String> output = ReadAll();

                await Task.Delay(TimeSpan.FromMilliseconds(50));

                result = new InteroResponse(output, currentErrors);
                LogLines("OUT >>>> ", output);
                LogLines("ERR >>>> ", currentErrors);
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
                result += c;
                if (result.EndsWith(EOT))
                {
                    result = result.Substring(0, result.Length - EOT.Length);
                    break;
                }
            }
            return result.Split('\n').Select(s => s.TrimEnd('\r')).ToList();
        }

        private void OnReceiveError(String error)
        {
            currentErrors.Add(error);
        }

        #endregion

        //-------- logging -------------
        String projectDir;
        private void LogLines(String prefix, List<String> xs)
        {
            String log = Path.Combine(projectDir, "intero.log");

            foreach (var x in xs)
            {
                File.AppendAllText(log, curRequest + ": " + prefix + x + "\n");
            }
        }

        private void LogLine(String prefix, String x)
        {
            LogLines(prefix, new String[]{ x }.ToList());
        }
    }
}

using Bearded.Monads;
using System;
using System.Diagnostics;

namespace FatalIDE.Core
{
    public static class Communication
    {
        public static EitherSuccessOrError<ProcessOutput, Error<String>> ReadProcess(String filename, String arguments, String workingDirectory = ".")
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                };
                return EitherSuccessOrError<String, Error<String>>.Create(
                    Process.Start(startInfo)
                           .StandardOutput
                           .ReadToEnd())
                    .Map(output => new ProcessOutput(output));
            }
            catch (Exception e)
            {
                return EitherSuccessOrError<ProcessOutput, Error<String>>.Create(
                    new Error<String>("When trying to call " + filename + ": " + e.Message));
            }
        }

        public static EitherSuccessOrError<Action<String>, Error<String>> StartProcess(
            String filename,
            String arguments,
            String workingDirectory,
            Action<String> StdHandler,
            Action<String> ErrHandler,
            Action ExitHandler)
        {
            try
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                };

                p.Start();

                p.OutputDataReceived += (o, a) => { if (a.Data != null) StdHandler(a.Data); };
                p.BeginErrorReadLine();
                p.ErrorDataReceived += (o, a) => { if (a.Data != null) ErrHandler(a.Data); };
                p.BeginErrorReadLine();
                p.Exited += (s, e) => ExitHandler();

                return EitherSuccessOrError<Action<String>, Error<String>>.Create(s => p.StandardInput.WriteLine(s));
            }
            catch (Exception e)
            {
                return EitherSuccessOrError<Action<String>, Error<String>>.Create(
                    new Error<String>("When trying to call " + filename
                                    + "at location '" + workingDirectory
                                    + "': " + e.Message));
            }
        }

        public static EitherSuccessOrError<Process, Error<String>> StartNewProcess(
            String filename,
            String arguments,
            String workingDirectory)
        {
            try
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                };

                p.Start();

                return EitherSuccessOrError<Process, Error<String>>.Create(p);
            }
            catch (Exception e)
            {
                return EitherSuccessOrError<Process, Error<String>>.Create(
                    new Error<String>("When trying to call " + filename
                                    + "at location '" + workingDirectory
                                    + "': " + e.Message));
            }
        }
    }
}

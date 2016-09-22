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
using System.Timers;

namespace FatalHaskell.External
{
    class FHIntero
    {
        ///////////////////////////////////////////////////////////////////////
        #region Constructor
        ///////////////////

        private static FHIntero Singleton = null;

        public static EitherSuccessOrError<FHIntero, Error<String>> Instance(String fileInProject)
        {
            if (Singleton == null)
            {
                // prepare configuration
                String projectDir = ProjectTree.FindProjectDir(fileInProject);
                String projectName = Path.GetFileName(projectDir);
                String mirrorProjectDir = Path.Combine(Path.GetTempPath(), "FatalDream", "FatalHaskell", projectName);

                // prepare mirror project
                ProjectTree.CopyDirectoryRec(projectDir, mirrorProjectDir);

                // create object
                FHIntero intero = new FHIntero(projectDir, mirrorProjectDir);

                // start process
                return Communication.StartNewProcess(
                       "stack", "ghci --with-ghc intero --ghci-options -ferror-spans",
                       mirrorProjectDir)
                     .WhenSuccess(p =>
                     {
                         p.ErrorDataReceived += (e, s) => intero.OnReceiveError(s.Data);
                         p.BeginErrorReadLine();
                         p.StandardInput.WriteLine(":set prompt >");
                         ReadAll(p);
                     })

                     .Select(p => intero.Initialize(p))

                     .WhenSuccess(index => Singleton = index)
                     .WhenError(err => MessageBox.Show(err.ToString()));
            }
            else
            {
                return EitherSuccessOrError<FHIntero, Error<String>>.Create(Singleton);
            }
        }

        private FHIntero(String projectDir, String mirrorProjectDir)
        {
            this.Errors = new ErrorContainer(mirrorProjectDir);
            this.projectDir = projectDir;
            this.mirrorProjectDir = mirrorProjectDir;
        }
        #endregion Constructor


        ///////////////////////////////////////////////////////////////////////
        #region Init
        /////////////

        //---- public ----
        public ErrorContainer Errors;

        //----- private -----
        //-- communiction --
        private Process CurrentProcess;
        //-- mirroring --
        private String projectDir;
        private String mirrorProjectDir;
        private Dictionary<String, String> mirrorFiles;
        private Timer mirrorTimer;


        private FHIntero Initialize(Process CurrentProcess)
        {
            this.CurrentProcess = CurrentProcess;

            this.mirrorFiles = new Dictionary<String, String>();

            this.mirrorTimer = new Timer(500);
            this.mirrorTimer.Elapsed += SaveMirrorProject;
            this.mirrorTimer.Start();
            
            return this;
        }

        #endregion Init

        ///////////////////////////////////////////////////////////////////////
        #region Mirroring
        /////////////////
        private void SaveMirrorProject(object sender, ElapsedEventArgs e)
        {
            lock (mirrorFiles)
            {
                if (mirrorFiles.Count > 0)
                {

                    foreach (var pathDiff_And_File in mirrorFiles)
                    {
                        String mirrorFile = mirrorProjectDir + pathDiff_And_File.Key;
                        File.WriteAllText(mirrorFile, pathDiff_And_File.Value);
                    }
                    mirrorFiles.Clear();

                    GetResponse(":r");
                }
            }
        }

        #endregion Mirroring


        public List<String> GetCompletions()
        {
            return new String[] { "first", "second", "third" }.ToList();
        }

        public List<String> UpdateFile(String relativeFilename, String contents)
        {
            lock (mirrorFiles)
            {
                mirrorFiles[relativeFilename] = contents;
            }
            return GetResponse(":r");
        }
    

        private List<String> GetResponse(String request)
        {
            Errors.Clear();
            CurrentProcess.StandardInput.WriteLine(request);
            List<String> response = ReadAll();
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

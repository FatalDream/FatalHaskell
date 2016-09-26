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
using System.Threading;

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
                String fatalTempPath = Path.Combine(Path.GetTempPath(), "FatalDream", "FatalHaskell");
                MirrorDirectories mirrorDirs = new MirrorDirectories(
                    projectDir,
                    Path.Combine(fatalTempPath, "Direct", projectName),
                    Path.Combine(fatalTempPath, "Correct", projectName));

                // prepare mirror project
                ProjectTree.CopyDirectoryRec(mirrorDirs.original, mirrorDirs.direct);
                ProjectTree.CopyDirectoryRec(mirrorDirs.original, mirrorDirs.correct);

                // create object
                FHIntero intero = new FHIntero(mirrorDirs);

                // start process
                var directProcess = InteroProcess.StartNew(mirrorDirs.direct);
                var correctProcess = InteroProcess.StartNew(mirrorDirs.correct);

                return directProcess.ZipM(correctProcess)
                     .Select(ps => intero.Initialize(ps.Item1, ps.Item2))

                     .WhenSuccess(index => Singleton = index)
                     .WhenError(err => MessageBox.Show(err.ToString()));
            }
            else
            {
                return EitherSuccessOrError<FHIntero, Error<String>>.Create(Singleton);
            }
        }

        private FHIntero(MirrorDirectories mirrorDirs)
        {
            this.Errors = new ErrorContainer(mirrorDirs.direct);
            this.mirrorDirs = mirrorDirs;
        }
        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Init
        /////////////

        //---- public ----
        public ErrorContainer Errors;

        //----- private -----
        //-- communiction --
        private InteroProcess directProcess;
        private InteroProcess correctProcess;
        //-- mirroring --
        private MirrorDirectories mirrorDirs;
        private Dictionary<String, String> filesToMirrorDirect;
        private Dictionary<String, String> filesToMirrorCorrect;
        private System.Timers.Timer mirrorTimer;


        private FHIntero Initialize(InteroProcess directProcess, InteroProcess correctProcess)
        {
            this.directProcess = directProcess;
            this.correctProcess = correctProcess;

            this.filesToMirrorDirect = new Dictionary<String, String>();
            this.filesToMirrorCorrect = new Dictionary<String, String>();

            this.mirrorTimer = new System.Timers.Timer(500);
            this.mirrorTimer.Elapsed += (s, e) => UpdateDirectMirrorProject();
            this.mirrorTimer.Start();

            this.directMirrorSemaphore = new SemaphoreSlim(1, 1);
            
            return this;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Mirroring
        /////////////////

        SemaphoreSlim directMirrorSemaphore;

        private async Task UpdateDirectMirrorProject()
        {
            var task = directMirrorSemaphore.WaitAsync();
            if (await Task.WhenAny(task, Task.Delay(5)) == task)
            {
                try
                {
                    CopyMirrorFiles(mirrorDirs.original, mirrorDirs.direct, filesToMirrorDirect);
                }
                finally
                {
                    directMirrorSemaphore.Release();
                }
            }
            else
            {
                directMirrorSemaphore.Release();
            }


            var r = await directProcess.GetResponse(":r");
            HandleErrors(r.error);

            if (r.error.Count == 0)
            {
                await SaveCorrectMirrorProject();
            }

        }
        private async Task SaveCorrectMirrorProject()
        {
            if (filesToMirrorCorrect.Count > 0)
            {
                await directMirrorSemaphore.WaitAsync();
                try
                {
                    CopyMirrorFiles(mirrorDirs.direct, mirrorDirs.correct, filesToMirrorCorrect);
                    await correctProcess.GetResponse(":r");
                }
                finally
                {
                    directMirrorSemaphore.Release();
                }
            }
        }

        private Option<Success> CopyMirrorFiles(String sourceBaseDir, String targetBaseDir, Dictionary<String,String> filesToCopy)
        {
            lock (filesToCopy)
            {
                if (filesToCopy.Count > 0)
                {

                    foreach (var pathDiff_And_File in filesToCopy)
                    {
                        String mirrorFile = mirrorDirs.direct + pathDiff_And_File.Key;
                        File.WriteAllText(mirrorFile, pathDiff_And_File.Value);
                    }
                    filesToCopy.Clear();

                    return new Success();
                }
                else
                {
                    return Option<Success>.None;
                }
            }
        }

        public void UpdateFile(String relativeFilename, String contents)
        {
            lock (filesToMirrorDirect)
            {
                filesToMirrorDirect[relativeFilename] = contents;
            }
            lock (filesToMirrorCorrect)
            {
                filesToMirrorCorrect[relativeFilename] = contents;
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Completion
        /////////////////

        public async Task<List<String>> GetCompletions(String relativeFilename, String curWord)
        {
            String filename = mirrorDirs.correct + relativeFilename;
            curWord = curWord.Trim();

            // TODO: fix this crude re-sync hack
            InteroResponse response;
            do
            {
                response = await correctProcess.GetResponse(":complete-at " + filename + " 1 1 1 1 \"" + curWord + "\"");
            } while (response.output[0].StartsWith("Ok, modules"));

            return response.output;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region ErrorHandling
        /////////////////

        public event Action<ErrorContainer> ErrorsChanged;

        public void HandleErrors(List<String> errors)
        {
            this.Errors.Clear();
            foreach (var error in errors)
            {
                this.Errors.AppendOrCreate(error);
            }

            ErrorsChanged?.Invoke(Errors);
        }

        #endregion
    }
}

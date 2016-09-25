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
        private Timer mirrorTimer;


        private FHIntero Initialize(InteroProcess directProcess, InteroProcess correctProcess)
        {
            this.directProcess = directProcess;
            this.correctProcess = correctProcess;

            this.filesToMirrorDirect = new Dictionary<String, String>();
            this.filesToMirrorCorrect = new Dictionary<String, String>();

            this.mirrorTimer = new Timer(500);
            this.mirrorTimer.Elapsed += (s, e) => UpdateDirectMirrorProject();
            this.mirrorTimer.Start();
            
            
            return this;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Mirroring
        /////////////////
        private async Task UpdateDirectMirrorProject()
        {
            CopyMirrorFiles(mirrorDirs.original, mirrorDirs.direct, filesToMirrorDirect);
            var r = await directProcess.GetResponse(":r");
            HandleErrors(r.error);

            if (r.error.Count == 0)
            {
                SaveCorrectMirrorProject();
            }
        }
        private void SaveCorrectMirrorProject()
        {
            CopyMirrorFiles(mirrorDirs.original, mirrorDirs.correct, filesToMirrorCorrect);
            correctProcess.GetResponse(":r");
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

        public async Task<List<String>> GetCompletions(String relativeFilename)
        {
            String filename = mirrorDirs.correct + relativeFilename;
            var response = await correctProcess.GetResponse(":complete-at " + filename + " 1 1 1 1 \"\"");
            return response.output;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region ErrorHandling
        /////////////////

        public void HandleErrors(List<String> errors)
        {
            this.Errors.Clear();
            foreach (var error in errors)
            {
                this.Errors.AppendOrCreate(error);
            }
        }

        #endregion
    }
}

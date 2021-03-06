﻿using System;
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
    class Intero
    {
        ///////////////////////////////////////////////////////////////////////
        #region Constructor
        ///////////////////

        private static Intero Singleton = null;

        public static EitherSuccessOrError<Intero, Error<String>> Instance(String fileInProject)
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
                Intero intero = new Intero(mirrorDirs);

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
                return EitherSuccessOrError<Intero, Error<String>>.Create(Singleton);
            }
        }

        private Intero(MirrorDirectories mirrorDirs)
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


        private Intero Initialize(InteroProcess directProcess, InteroProcess correctProcess)
        {
            this.directProcess = directProcess;
            this.correctProcess = correctProcess;

            this.filesToMirrorDirect = new Dictionary<String, String>();
            this.filesToMirrorCorrect = new Dictionary<String, String>();

            this.mirrorTimer = new System.Timers.Timer(500);
            this.mirrorTimer.Elapsed += (s, e) => UpdateDirectMirrorProject();
            this.mirrorTimer.Start();

            InitializeInsights();
            
            return this;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Mirroring
        /////////////////

        private async Task UpdateDirectMirrorProject()
        {
            WriteMirrorFiles(mirrorDirs.direct, filesToMirrorDirect);
    
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
                WriteMirrorFiles(mirrorDirs.correct, filesToMirrorCorrect);
                await correctProcess.GetResponse(":r");
            }
        }

        private static Option<Success> WriteMirrorFiles(String targetBaseDir, Dictionary<String,String> filesToCopy)
        {
            lock (filesToCopy)
            {
                if (filesToCopy.Count > 0)
                {

                    foreach (var pathDiff_And_File in filesToCopy)
                    {
                        String mirrorFile = targetBaseDir + pathDiff_And_File.Key;
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

        ///////////////////////////////////////////////////////////////////////
        #region Insights
        /////////////////

        List<String> Keywords;

        void InitializeInsights()
        {
            Keywords = new String[]
            {
                "module", "where", "import",
                "if", "then", "else", "do",
                "newtype", "class", "instance"
            }.ToList();
        }

        public IEnumerable<String> GetKeywords()
        {
            return Keywords;
        }

        #endregion
    }
}

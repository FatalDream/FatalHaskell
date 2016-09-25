using Bearded.Monads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalIDE.Core
{
    class ProjectTree
    {
        public static Option<String> RelativeFilename(String filename)
        {
            String projectDir = ProjectTree.FindProjectDir(filename);
            return ProjectTree.GetPathDiff(projectDir, filename);
        }

        public static String GetDirWithFile(String somePath, String fileName)
        {
            if (Path.HasExtension(somePath))
            {
                somePath = Path.GetDirectoryName(somePath);
            }

            if (Directory.EnumerateFiles(somePath).Any(f => Path.GetFileName(f) == fileName))
            {
                return somePath;
            }
            else
            {
                return GetDirWithFile(Directory.GetParent(somePath).FullName, fileName);
            }
        }

        public static String FindProjectDir(String file)
        {
            return GetDirWithFile(file, "stack.yaml");
        }

        public static String GetCommonPath(params String[] Files)
        {
            var MatchingChars =
            from len in Enumerable.Range(0, Files.Min(s => s.Length)).Reverse()
            let possibleMatch = Files.First().Substring(0, len)
            where Files.All(f => f.StartsWith(possibleMatch))
            select possibleMatch;

            return Path.GetDirectoryName(MatchingChars.First());
        }

        public static Option<String> GetPathDiff(String basePath, String extendedPath)
        {
            String result = "";

            int i;
            for (i = 0; i < basePath.Length; i++)
            {
                if (i >= extendedPath.Length || char.ToLower(basePath[i]) != char.ToLower(extendedPath[i]))
                    return Option<String>.None;
            }
            return extendedPath.Substring(i);
        }

        public static void CopyDirectoryRec(String sourceDir, String targetDir)
        {
            Console.WriteLine("\nCopying Directory:\n  \"{0}\"\n-> \"{1}\"", sourceDir, targetDir);

            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
            string[] sysEntries = Directory.GetFileSystemEntries(sourceDir);

            foreach (string sysEntry in sysEntries)
            {
                string fileName = Path.GetFileName(sysEntry);
                string targetPath = Path.Combine(targetDir, fileName);
                if (Directory.Exists(sysEntry))
                    CopyDirectoryRec(sysEntry, targetPath);
                else
                {
                    Console.WriteLine("\tCopying \"{0}\"", fileName);
                    File.Copy(sysEntry, targetPath, true);
                }
            }
        }
    }
}

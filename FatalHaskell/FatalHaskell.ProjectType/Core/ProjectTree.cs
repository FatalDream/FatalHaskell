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
    }
}

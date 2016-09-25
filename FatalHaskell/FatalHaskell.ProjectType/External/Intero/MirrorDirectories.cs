using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatalHaskell.External
{
    class MirrorDirectories
    {
        public MirrorDirectories(String original, String direct, String correct)
        {
            this.original = original;
            this.direct = direct;
            this.correct = correct;
        }

        /// <summary>
        /// Path to the original project location.
        /// </summary>
        public readonly String original;

        /// <summary>
        /// Path to directory where the project is mirrored as directly as possible.
        /// </summary>
        public readonly String direct;

        /// <summary>
        /// Path to directory where the project is only mirrored, if it contains no errors.
        /// </summary>
        public readonly String correct;
    }
}

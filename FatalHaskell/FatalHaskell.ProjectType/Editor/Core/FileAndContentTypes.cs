using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace FatalHaskell.Editor.Core
{
    internal static class FileAndContentTypeDefinitions
    {
        [Export]
        [Name("hs")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition hidingContentTypeDefinition;

        [Export]
        [FileExtension(".hs")]
        [ContentType("hs")]
        internal static FileExtensionToContentTypeDefinition hiddenFileExtensionDefinition;
    }
}

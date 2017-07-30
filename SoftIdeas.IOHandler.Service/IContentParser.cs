using System;
using System.IO;

namespace SoftIdeas.IOHandler.Service
{
    /// <summary>
    /// Service "facade service to encapsulate internal adapters, and business logic.
    /// </summary>
    public interface IContentParser : IDisposable
    {
        void CopyContent(FileInfo sourceFileInfo, DirectoryInfo destinationDirectoryInfo, Action<Exception> onError, Action<FileInfo> onComplete);
        /// <summary>
        /// Overload to apply transformers over parser, without touch the logic of the parser, this feature should only decorate "On Next" handler to get data before and after respective parser to adapt contents. 
        /// </summary>
        /// <param name="sourceFileInfo"></param>
        /// <param name="destinationDirectoryInfo"></param>
        /// <param name="onError"></param>
        /// <param name="onComplete"></param>
        /// <param name="transformars"></param>
        void CopyContent(FileInfo sourceFileInfo, DirectoryInfo destinationDirectoryInfo, Action<Exception> onError, Action<FileInfo> onComplete, params Func<string, string>[] transformars);
    }
}

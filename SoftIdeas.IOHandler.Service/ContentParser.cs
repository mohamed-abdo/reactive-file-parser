using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftIdeas.IOHandler.Adaptee;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftIdeas.IOHandler.Service
{
    /// <summary>
    /// Content parser Service, to read file info, initiate adequate content parser (adapter) based on file extension, then copy parsed contents to a destination.
    /// </summary>
    public class ContentParser : IContentParser
    {
        private ILogger _logger;
        private IServiceProvider _serviceProvider;
        public ContentParser()
        {
            _serviceProvider = new ServiceCollection()
               .AddLogging()
               .AddSingleton<IContentParserReactiveAdapter, JSAdapter.JSParser>()
               .AddSingleton<IContentParserReactiveAdapter, CSSAdapter.CSSParser>()
               .BuildServiceProvider();

            _logger = _serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug)
                .CreateLogger<ContentParser>();
            _logger.LogInformation("ContentParser just initiated!");
        }

        public void Dispose()
        {
            _logger.LogInformation("ContentParser is going to dispose!");
            _logger = null;
            _serviceProvider = null;
        }

        public void CopyContent(FileInfo sourceFileInfo, DirectoryInfo destinationDirectoryInfo, Action<Exception> onError, Action<FileInfo> onComplete)
        {
            #region validation

            if (sourceFileInfo == null)
            {
                _logger.LogInformation("null value of file info is invalid!");
                onError(new ArgumentNullException("null value of file info is invalid!"));
            }
            if (!sourceFileInfo.Exists)
            {
                _logger.LogInformation($"file {sourceFileInfo.Name} is not exists!");
                onError(new ArgumentException($"file {sourceFileInfo.Name} is not exists!"));
            }
            if (destinationDirectoryInfo == null)
            {
                _logger.LogInformation("null value of destination info is invalid!");
                onError(new ArgumentNullException("null value of destination info is invalid!"));
            }

            #endregion

            FileStream destinationFileStream = null;
            StreamWriter destinationFileWriter = null;

            try
            {
                if (!destinationDirectoryInfo.Exists)
                {
                    _logger.LogInformation($"directory {sourceFileInfo.Name} is not exists, and it will be created by content service.");
                    destinationDirectoryInfo.Create();
                }

                //check if file exits in destination directory
                if (destinationDirectoryInfo.GetFileSystemInfos().Any(file => file.Name.ToLower() == sourceFileInfo.Name.ToLower()))
                {
                    _logger.LogWarning($"file {sourceFileInfo.Name} exits in destination directory, task will be canceled.");
                    return;
                }
                IContentParserReactiveAdapter adapter = null;

                var fileFullName = Path.Combine(destinationDirectoryInfo.FullName, sourceFileInfo.Name);

                _logger.LogInformation($"working in file: {fileFullName}.");
                // open file to write
                destinationFileStream = File.OpenWrite(fileFullName);
                destinationFileWriter = new StreamWriter(destinationFileStream, Encoding.UTF8);
                // get adequate adapter for file contents
                switch (sourceFileInfo.Extension?.ToLower())
                {
                    case ".js":
                        {
                            adapter = ContentParserAdapterFactory.GetReactiveAdapter(_serviceProvider, FileType.JS);
                            break;
                        };
                    case ".css":
                        {
                            adapter = ContentParserAdapterFactory.GetReactiveAdapter(_serviceProvider, FileType.CSS);
                            break;
                        };
                    default:
                        {
                            _logger.LogInformation($"file {sourceFileInfo.Extension} extension is not supported.");
                            onError(new NotSupportedException($"file {sourceFileInfo.Extension} extension is not supported."));
                            return;
                        };
                }
                // content reader / writer
                int currentContentProgress = 0;
                adapter.ParseContent(
                    sourceFileInfo.OpenRead(),
                    //on error handler
                    (err) =>
                    {
                        _logger.LogError(err.Message);
                        destinationFileWriter.Dispose();
                        destinationFileStream.Dispose();
                        throw err;
                    },
                    //on receiving chunk contents, and send feedback to caller.
                    (contentAsBytes) =>
                    {
                        try
                        {
                            var content = Encoding.UTF8.GetString(contentAsBytes);
                            destinationFileWriter?.Write(content);
                            destinationFileWriter.Flush();
                            _logger.LogInformation($"writing data, .... receiving contents of {currentContentProgress += content.Length}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                            destinationFileWriter.Dispose();
                            return Feedback.Failed;
                        }
                        return Feedback.Wait | Feedback.Succeed;
                    },
                    // on complete task, close stream (clean any locked resources)
                    (readingStream) =>
                    {
                        _logger.LogInformation($"file contents {readingStream.Length} has been written successfully to destination, stream will be closed!");
                        readingStream.Dispose();
                        onComplete(new FileInfo(fileFullName));
                    },
                    // 1000 bytes as chunk size in terms of bytes.
                    chunkSize: 1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                onError(ex);
            }
            finally
            {
                destinationFileWriter.Dispose();
                destinationFileStream.Dispose();
            }
        }

        public void CopyContent(FileInfo sourceFileInfo, DirectoryInfo destinationDirectoryInfo, Action<Exception> onError, Action<FileInfo> onComplete, params Func<string, string>[] transformars)
        {
            throw new NotImplementedException();
        }
    }
}

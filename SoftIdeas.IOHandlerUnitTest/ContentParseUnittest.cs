using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftIdeas.IOHandler.Service;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Bogus;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SoftIdeas.IOHandlerUnitTest
{
    [TestClass]
    public sealed class ContentParseUnitTest
    {
        /// <summary>
        /// Unit test
        /// </summary>
        private const string sourceFolderPath = "c:\\IOHandlerSource";
        private const string destinationFolderPath = "c:\\IOHandlerDestination";
        private readonly string[] extensions = new string[] { ".js", ".css" };
        private DirectoryInfo sourceDirectory, destinationDirectory;

        private Faker _fakeContents;
        private IContentParser _contentParserService;
        private ILogger _logger;
        public ContentParseUnitTest()
        {
            // configure services injection.
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IContentParser, ContentParser>()
                .BuildServiceProvider();
            _fakeContents = new Faker();
            _logger = serviceProvider
                 .GetService<ILoggerFactory>()
                 .CreateLogger(typeof(ContentParseUnitTest));
            // get required service
            _contentParserService = serviceProvider.GetService<IContentParser>();
        }

        /// <summary>
        /// Steps: 
        ///     1- Create source folder
        ///     2- add file.css with contents
        ///     3- add file.js with contents
        ///     4- create destination folder    
        ///     5- get service instance
        /// </summary>
        [TestInitialize]
        public void SetupEnvironment()
        {
            FileStream fsCSS = null, fsJS = null;
            TextWriter CSSWriter = null, JSWriter = null;
            try
            {
                //1 - Create source folder
                sourceDirectory = Directory.CreateDirectory(sourceFolderPath);
                //2 - add file.css with contents
                fsCSS = File.OpenWrite(Path.Combine(sourceDirectory.FullName, $"{DateTime.Now.ToFileTimeUtc()}.css"));
                CSSWriter = new StreamWriter(fsCSS);
                CSSWriter.Write(_fakeContents.Lorem.Paragraphs(10));

                //3 - add file.js with contents
                fsJS = File.OpenWrite(Path.Combine(sourceDirectory.FullName, $"{DateTime.Now.ToFileTimeUtc()}.js"));
                JSWriter = new StreamWriter(fsJS);
                JSWriter.Write(_fakeContents.Lorem.Paragraphs(10));

                //4 - create destination folder
                destinationDirectory = Directory.CreateDirectory(destinationFolderPath);
                //5- get service instance
                //_contentParserService should be initialized at constructor
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            finally
            {
                CSSWriter.Dispose();
                fsCSS.Dispose();

                JSWriter.Dispose();
                fsJS.Dispose();
            }

        }
        /// <summary>
        /// Steps:
        ///  1- dispose the service
        ///  2- Delete source folder, with its contents.
        ///  3- Delete destination folder, with its contents.(i keep destination just for find application side effects)
        /// </summary>
        [TestCleanup]
        public void CleanupEnvionment()
        {
            //1 - dispose service
            _contentParserService.Dispose();
            //2 - Delete source folder, with its contents.
            sourceDirectory.Delete(true);
            //3 - Delete destination folder, with its contents.
            // destinationDirectory.Delete(true);
        }
        /// <summary>
        /// Test (happy scenario) for content parsing
        /// </summary>
        [TestMethod]
        public void ContnetParserTest()
        {
            // read files from source directory, where files extensions are supported.
            sourceDirectory.EnumerateFiles()
                .Where(x => extensions.Any(ex => ex == x.Extension))
                .ToList()
                .ForEach(fileInfo =>
                {
                    // calling the service
                    _contentParserService.CopyContent(fileInfo, destinationDirectory, err =>
                    {
                        // on error handler
                        _logger.LogError(err.Message);
                    }, completedFileInfo =>
                    {
                        // on complete handler
                        _logger.LogInformation($"file {completedFileInfo.Name} has been successfully copies to {destinationDirectory.FullName}.");
                    });
                });
        }
    }
}

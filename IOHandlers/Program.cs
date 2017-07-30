using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftIdeas.IOHandler.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftIdeas.IOHandlers.Shell
{
    class Program
    {
        /// <summary>
        /// Shell program to read files (currently JS, and CSS) contents from a folder, and then write those files to a certain location.
        /// <paramref name="args"/>     Expected parameters are: 
        ///                     1) a valid folder path to read files from its root path.
        ///                     2) a valid path to write JS files contents.
        ///                     3) a valid path to write CSS files contents.
        /// </summary>
        /// Created by  :   Mohamed Abdo, Mohamad.Abdo@gmail.com
        /// Modified on :   2017-07-29
        static void Main(string[] args)
        {
            Console.WriteLine("Application startup!");
            if (args.Length < 2)
            {
                Console.WriteLine("Source & Destination path are required parameters, Please re execute the program with required arguments!");
                Console.WriteLine("Press enter to exit!");
                Console.ReadLine();
                return;
            }
            ILogger _logger = null;
            try
            {
                var serviceProvider = getBootStrapProvider();
                //configure logger service
                _logger = serviceProvider
                    .GetService<ILoggerFactory>()
                    .AddConsole(LogLevel.Debug)
                    .CreateLogger<Program>();

                // getting required parameters
                var sourceDirectoryPath = args[0];
                _logger.LogInformation($"source directory : {sourceDirectoryPath}");
                var destinationDirectoryPath = args[1];
                _logger.LogInformation($"destination directory : {destinationDirectoryPath}");

                var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
                var destinationDirectory = new DirectoryInfo(destinationDirectoryPath);

                // Get content parser service
                var fileService = serviceProvider.GetService<IContentParser>();

                // hard code supported extensions
                var extensions = new string[] { ".js", ".css" };

                // read files from source directory, where files extensions are supported.
                sourceDirectory.EnumerateFiles()
                    .Where(x => extensions.Any(ex => ex == x.Extension))
                    .ToList()
                    .ForEach(fileInfo =>
                    {
                        // calling the service
                        fileService.CopyContent(fileInfo, destinationDirectory, err =>
                        {
                            // on error handler
                            _logger.LogError(err.Message);
                        }, completedFileInfo =>
                        {
                            // on complete handler
                            _logger.LogInformation($"file {completedFileInfo.Name} has been successfully copies to {destinationDirectory.FullName}.");
                        });
                    });

                _logger.LogDebug("Application execution, done #");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Press enter to exit!");
            Console.ReadLine();
        }

        #region private
        /// <summary>
        /// bootstrap shell dependencies, and get IServiceProvider to locate injected services.
        /// </summary>
        private static Func<IServiceProvider> getBootStrapProvider = () =>
        {
            Startup startup = new Startup();
            IServiceCollection services = new ServiceCollection();
            startup.ConfigureServices(services);
            return services.BuildServiceProvider();
        };

        #endregion
    }
}
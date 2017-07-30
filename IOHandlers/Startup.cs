using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftIdeas.IOHandler.Service;

namespace SoftIdeas.IOHandlers.Shell
{
    public class Startup
    {
        IConfigurationRoot Configuration { get; }

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json");

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddLogging();
            services.AddSingleton<IContentParser, ContentParser>();
        }

    }
}

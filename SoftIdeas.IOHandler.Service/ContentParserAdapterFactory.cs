using Microsoft.Extensions.DependencyInjection;
using SoftIdeas.IOHandler.Adaptee;
using System;
using System.Linq;

namespace SoftIdeas.IOHandler.Service
{
    internal enum FileType
    {
        JS,
        CSS
    }
    internal static class ContentParserAdapterFactory
    {
        public static IContentParserAdapter GetAdapter(IServiceProvider serviceProvider, FileType fileType)
        {
            IContentParserAdapter service = null;
            switch (fileType)
            {
                case FileType.JS:
                    service = serviceProvider.GetServices<IContentParserAdapter>().FirstOrDefault(srv => srv is JSAdapter.JSParser);
                    break;
                case FileType.CSS:
                    service = serviceProvider.GetServices<IContentParserAdapter>().FirstOrDefault(srv => srv is CSSAdapter.CSSParser);
                    break;
                default:
                    throw new NotImplementedException("Not implemented parser!");
            }
            return service;
        }

        public static IContentParserReactiveAdapter GetReactiveAdapter(IServiceProvider serviceProvider, FileType fileType)
        {
            IContentParserReactiveAdapter service = null;
            switch (fileType)
            {
                case FileType.JS:
                    service = serviceProvider.GetServices<IContentParserReactiveAdapter>().FirstOrDefault(srv => srv is JSAdapter.JSParser);
                    break;
                case FileType.CSS:
                    service = serviceProvider.GetServices<IContentParserReactiveAdapter>().FirstOrDefault(srv => srv is CSSAdapter.CSSParser);
                    break;
                default:
                    throw new NotImplementedException("Not implemented parser!");
            }
            return service;
        }
    }
}

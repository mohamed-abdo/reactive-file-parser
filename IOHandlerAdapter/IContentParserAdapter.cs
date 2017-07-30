using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Design Decision: Returns stream content as bytes instead of string, since we may add adapters that handle content types rather than string, ex., pdf | excel files.
/// </summary>
namespace SoftIdeas.IOHandler.Adaptee
{
    /// <summary>
    /// Safe stream parser, and return contents as task of bytes array.
    /// In case of exception, on error method will be called and exception context will be injected as parameter.
    /// </summary>
    public interface IContentParserAdapter
    {
        Task<byte[]> ParseContent(Stream stream, Action<Exception> onError);
    }

    /// <summary>
    /// Safe stream reactive parser, and return array of bytes contents on next callback, suitable for large stream contents.
    /// In case of exception, on error method will be called and exception context will be injected as parameter.
    /// onNext chunk will pass contents bytes array of contents and return feedback, feedback may takes one or more flags, to control execution follow, ex., synchronize operation.
    /// onComplete will be triggered when all stream content has been read, and feedback was succeed for all of onNext
    /// bytes chunk to adjust function threshold, or when On next result will be triggered.
    /// </summary>
    public interface IContentParserReactiveAdapter
    {
        void ParseContent(Stream stream, Action<Exception> onError, Func<byte[], Feedback> onNext, Action<Stream> onComplete, int chunkSize = 1000);
    }
}

using SoftIdeas.IOHandler.Adaptee;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SoftIdeas.IOHandler.CSSAdapter
{
    public class CSSParser : BaseContentParserAdaptee, IContentParserAdapter, IContentParserReactiveAdapter
    {
        protected override string ContentType => "css";
        
        /// <summary>
        /// CSS contents parser for stream contents
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="onError">In case of exception, on error method will be called and exception context will be injected as parameter.</param>
        /// <returns>return stream contents as  array of bytes</returns>
        public Task<byte[]> ParseContent(Stream stream, Action<Exception> onError)
        {
            return ReadStreamContent(stream, onError);
        }

        /// <summary>
        /// CSS contents parser for stream contents reactively, and return array of bytes contents on next callback once chunk size filled out, suitable for large stream contents.
        /// </summary>
        /// <param name="stream">stream to read data</param>
        /// <param name="onError">In case of exception, on error method will be called and exception context will be injected as parameter.</param>
        /// <param name="onNext">onNext Chunk will pass contents bytes array of contents and return feedback, feedback may takes one or more flags, to control execution follow, ex., synchronize operation.</param>
        /// <param name="onComplete">onComplete will be triggered when all stream content has been read, and feedback was succeed for all of onNext</param>
        /// <param name="chunkSize">bytes chunk size to adjust function threshold, or when On next result will be triggered.</param>
        public void ParseContent(Stream stream, Action<Exception> onError, Func<byte[], Feedback> onNext, Action<Stream> onComplete, int chunkSize)
        {
            ReadStreamContent(stream, onError, onNext, onComplete, chunkSize);
        }
    }
}

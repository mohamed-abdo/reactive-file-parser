using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftIdeas.IOHandler.Adaptee
{
    /// <summary>
    /// Adaptee base class provides base functionality for content parser adapters.
    /// </summary>
    public abstract class BaseContentParserAdaptee
    {
        #region Abstract
        /// <summary>
        /// Adapter must override this property to declare its target content type.
        /// </summary>
        protected abstract string ContentType { get; }

        #endregion

        #region Virtual

        /// <summary>
        /// Iterate over input stream to lazy yield lines.
        /// </summary>
        /// <param name="stream">stream parameter to get input</param>
        /// <exception cref="ArgumentNullException">in case of null value passed to stream parameter</exception>
        /// <returns>IEnumerable of string</returns>
        protected virtual IEnumerable<byte[]> GetChunkContents(Stream stream, int chunkSize)
        {
                if (stream == null)
                    throw new ArgumentNullException("null value for stream parameter is invalid!");
                long fileSize = stream.Length;
                var content = new byte[chunkSize];
                int idx = 0;
                while (stream.CanRead)
                {
                    if ((fileSize - idx) > chunkSize)
                    {
                        stream.Read(content, 0, chunkSize);
                        idx += chunkSize;
                        yield return content;
                        content = new byte[chunkSize];
                    }
                    else
                    {
                        stream.Read(content, 0, (int)(fileSize - idx));
                        yield return content.Take((int)(fileSize - idx)).ToArray();
                        content = null;
                        break;
                    }
                }
        }

        /// <summary>
        ///  Read stream contents as array of bytes
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual byte[] GetBytesContents(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream parameter has invalid value!");
            return new BinaryReader(stream)?.ReadBytes((int)stream.Length);
        }
        /// <summary>
        /// Split content into lines and lazy yield lines.
        /// </summary>
        /// <param name="content">string parameter</param>
        /// <exception cref="ArgumentNullException">in case of null value passed to content parameter</exception>
        /// <returns>IEnumerable of string</returns>
        protected virtual IEnumerable<string> GetContentLines(string content)
        {
            const char spliter = '\n';
            if (content == null)
                throw new ArgumentNullException("content parameter has invalid value!");
            if (content.Split(spliter).Length == 0)
                yield return content;
            foreach (var line in content.Split(spliter))
            {
                yield return line;
            }
        }

        /// <summary>
        /// Read stream contents reactively, and return array of bytes contents on next callback once chunk size filled out, suitable for large stream contents.
        /// </summary>
        /// <param name="stream">stream to read data</param>
        /// <param name="onError">In case of exception, on error method will be called and exception context will be injected as parameter.</param>
        /// <param name="onNext">onNext Chunk will pass contents bytes array of contents and return feedback, feedback may takes one or more flags, to control execution follow, ex., synchronize operation.</param>
        /// <param name="onComplete">onComplete will be triggered when all stream content has been read, and feedback was succeed for all of onNext</param>
        /// <param name="chunkSize">bytes chunk size to adjust function threshold, or when On next result will be triggered.</param>
        protected virtual void ReadStreamContent(Stream stream, Action<Exception> onError, Func<byte[], Feedback> onNext, Action<Stream> onComplete, int chunkSize = 1000)
        {
            try
            {
                var iterator = GetChunkContents(stream, chunkSize).GetEnumerator();
                while (iterator.MoveNext())
                {
                    var feedback = onNext(iterator.Current);
                    //TODO: switch case over feedback result, to enable retry feature as an example. 
                }
                //reading all content done
                onComplete(stream);
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        }
        /// <summary>
        /// Read stream contents
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="onError">In case of exception, on error method will be called and exception context will be injected as parameter.</param>
        /// <returns>return stream contents as  array of bytes</returns>
        protected virtual Task<byte[]> ReadStreamContent(Stream stream, Action<Exception> onError)
        {
            try
            {
                return Task.FromResult(GetBytesContents(stream));
            }
            catch (Exception ex)
            {
                onError(ex);
                return null;
            }
        }
        #endregion

        #region Final
        /// <summary>
        /// Read stream all contents as string
        /// </summary>
        /// <param name="stream">stream parameter</param>
        /// <exception cref="ArgumentNullException">in case of null value passed to stream parameter</exception>
        /// <returns>string of all stream contents</returns>
        protected string ReadStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream parameter has invalid value!");
            return new StreamReader(stream, true)?.ReadToEnd();
        }

        /// <summary>
        /// Get bytes representation of unicode string
        /// </summary>
        /// <param name="contents">string in UTF8</param>
        /// <exception cref="ArgumentNullException">in case of null value passed to contents parameter</exception>
        /// <returns>bytes array</returns>
        protected byte[] ToBytes(string contents)
        {
            if (contents == null)
                throw new ArgumentNullException("contents parameter has invalid value!");
            return Encoding.UTF8.GetBytes(contents);
        }

        /// <summary>
        /// Get string representation of bytes
        /// </summary>
        /// <param name="contents">bytes array</param>
        /// <exception cref="ArgumentNullException">in case of null value passed to contents parameter</exception>
        /// <returns>string of contents</returns>
        protected string ToString(byte[] contents)
        {
            if (contents == null)
                throw new ArgumentNullException("contents parameter has invalid value!");
            return Encoding.UTF8.GetString(contents);
        }
        #endregion
    }
}

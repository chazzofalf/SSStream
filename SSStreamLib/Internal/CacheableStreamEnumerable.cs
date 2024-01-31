using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Xml.Linq;

namespace SStreamLib.Internal
{
    public partial class CacheableStreamEnumerable : IEnumerable<byte>, IDisposable
    {
        public Stream inputStream;
        private TempFile cacheFile;
        public CacheableStreamEnumerable(Stream inputstream,bool forDecoding=true)
        {
            inputStream = inputstream;
            if (forDecoding)
            {
                CenterAndCacheStream();
            }
            else
            {
                CacheStream();
            }
            
        }

        private void CacheStream()
        {
            cacheFile = new TempFile();
            var cache = cacheFile.OpenForWriting();
            inputStream.CopyTo(cache);
            inputStream.Flush();
            inputStream.Close();

        }

        private void FindMyCenter(out byte[] startBuffer)
        {
            //Find the "True Center" (this is the useful beginning) of file.
            //The "True" beginning is defined by the following sequence of bytes:
            // 54 0x36 (54 or '6') bytes
            // A sequence of bytes that is not four 0x00 (0 or NUL) bytes
            byte[] bytes = new byte[4096];
            byte[] magic = Enumerable.Repeat((byte)54, 54).ToArray();
            byte[] endblock = magic.Concat(Enumerable.Repeat((byte)0, 4)).ToArray();
            List<byte> slidingWindow = new List<byte>();
            var slidingWindowSize = endblock.Length;
            bool centerFound = false;
            var len = inputStream.Read(bytes);
            var slidingWindowIndex = 0;
            (var is_magic, var is_eof) = (false, false);
            while (len > 0 && !centerFound)
            {
                slidingWindowIndex = 0;
                while (slidingWindowIndex < bytes.Length)
                {
                    if (slidingWindow.Count == 0)
                    {
                        if (len >= slidingWindowSize)
                        {
                            slidingWindow.AddRange(bytes.Take(slidingWindowSize));
                            slidingWindowIndex = slidingWindowSize;
                        }
                        else
                        {
                            inputStream.Close();
                            throw new IOException("There is no center!", new ArgumentException());
                        }
                    }
                    else if (slidingWindow.Count == slidingWindowSize)
                    {
                        slidingWindow.RemoveAt(0);
                        slidingWindow.Append(bytes[slidingWindowIndex++]);
                    }
                    else
                    {
                        inputStream.Close();
                        throw new IOException("Sliding window too short!", new ArgumentException());
                    }
                    is_magic = slidingWindow.Count > magic.Length && !slidingWindow
                        .Take(magic.Length)
                        .Zip(magic, (srcbyte, testbyte) => srcbyte == testbyte)
                        .Where(test => !test)
                        .Any();
                    is_eof = slidingWindow.Count == endblock.Length && !slidingWindow
                        .Zip(endblock, (srcbyte, testbyte) => srcbyte == testbyte)
                        .Where(test => !test)
                        .Any();
                    if (is_magic && !is_eof)
                    {
                        centerFound = true;
                    }
                }
                if (!centerFound)
                {
                    len = inputStream.Read(bytes);
                }



            }
            if (!centerFound)
            {
                inputStream.Close();
                throw new IOException("Could Not find the center.", new ArgumentException());
            }
            else
            {
                startBuffer = slidingWindow
                    .Concat(bytes.Skip(slidingWindowIndex))
                    .ToArray();


            }

        }
        private void CenterAndCacheStream()
        {
            byte[] startBuffer = null;

            FindMyCenter(out startBuffer);
            cacheFile = new TempFile();
            CacheToFile(cacheFile, startBuffer);



        }

        private void CacheToFile(TempFile cacheFile, byte[] startBuffer)
        {
            byte[] bytes = new byte[4096];
            byte[] magic = Enumerable.Repeat((byte)54, 54).ToArray();
            byte[] endblock = magic.Concat(Enumerable.Repeat((byte)0, 4)).ToArray();
            List<byte> slidingWindow = new List<byte>();
            var slidingWindowSize = endblock.Length;
            bool endFound = false;
            var len = inputStream.Read(bytes);
            var found_beginning = false;
            var found_end = false;
            var found_beginning_prev = false;
            var found_end_prev = false;
            var slidingWindowIndex = 0;
            var cache_out = (Stream?)null;
            var cache_out_buffer = new List<byte>();
            (var is_magic, var is_eof) = (false, false);
            var iter = startBuffer.
                Concat(((Func<IEnumerable<byte>>)(() =>
                {
                    var o = Enumerable.Empty<byte>();
                    var buff = new byte[4096];
                    var lenx = inputStream.Read(buff);
                    while (lenx > 0)
                    {
                        o = o.Concat(buff.Take(lenx));
                        lenx = inputStream.Read(buff);
                    }
                    return o;
                }))())
                .Aggregate((Output: Enumerable.Empty<byte[]>(), Buffer: Enumerable.Empty<byte>(), Count: 0), (prev, current) =>
                {
                    var count = prev.Count;
                    var buffer = prev.Buffer;
                    var output = prev.Output;
                    buffer = buffer.Append(current);
                    count++;
                    if (count == 4096)
                    {
                        output.Append(buffer.ToArray());
                        count = 0;
                        buffer = Enumerable.Empty<byte>();
                    }
                    return (Output: output, Buffer: buffer, Count: count);
                },
                (fin) =>
                {
                    var count = fin.Count;
                    var buffer = fin.Buffer;
                    var output = fin.Output;
                    if (count > 0)
                    {
                        output.Append(buffer.ToArray());
                    }
                    return output;
                });
            foreach (var block in iter)
            {
                slidingWindowIndex = 0;
                if (slidingWindow.Count == 0)
                {
                    if (block.Length >= slidingWindowSize)
                    {
                        slidingWindow.AddRange(block.Take(slidingWindowSize));
                        slidingWindowIndex = slidingWindowSize;
                    }
                    else
                    {
                        inputStream.Close();
                        throw new IOException("There is no center!", new ArgumentException());
                    }
                }
                else if (slidingWindow.Count == slidingWindowSize)
                {
                    slidingWindow.RemoveAt(0);
                    slidingWindow.Append(bytes[slidingWindowIndex++]);
                }
                else
                {
                    inputStream.Close();
                    throw new IOException("Sliding window too short!", new ArgumentException());
                }
                is_magic = slidingWindow.Count > magic.Length && !slidingWindow
                        .Take(magic.Length)
                        .Zip(magic, (srcbyte, testbyte) => srcbyte == testbyte)
                        .Where(test => !test)
                        .Any();
                is_eof = slidingWindow.Count == endblock.Length && !slidingWindow
                    .Zip(endblock, (srcbyte, testbyte) => srcbyte == testbyte)
                    .Where(test => !test)
                    .Any();
                found_beginning_prev = found_beginning_prev || found_beginning;
                found_end_prev = found_end_prev || found_end;
                found_beginning = is_magic && !is_eof;
                found_end = is_magic && is_eof;
                if (!found_beginning_prev)
                {
                    
                    if (found_beginning)
                    {
                        cache_out = cacheFile.OpenForWriting();
                        cache_out_buffer.AddRange(slidingWindow);
                    }
                    else
                    {
                        found_end = is_magic && is_eof;
                        if (found_end)
                        {
                            inputStream.Close();
                            throw new IOException("Check FindMyCenter. This is not really the center.", new ArgumentException());
                        }
                    }

                }
                else if (!found_end_prev)
                {
                    if (found_end)
                    {
                        if (cache_out != null)
                        {
                            cache_out.Write(cache_out_buffer.ToArray());
                            cache_out.Flush();
                            cache_out.Close();
                            inputStream.Close();                            
                            break;
                        }
                        else
                        {
                            inputStream.Close();
                            throw new IOException("Check FindMyCenter. This is not really the center.", new ArgumentException());
                        }

                        
                    }
                    else
                    {
                        cache_out_buffer.Append(slidingWindow.Last());
                    }
                }
                found_beginning = is_magic && !is_eof;
            }

            
        }
        public IEnumerator<byte> GetEnumerator()
        {
            return new CachableStreamEnumerator(cacheFile.OpenForReading());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CachableStreamEnumerator(cacheFile.OpenForReading());
        }

        public void Dispose()
        {
            cacheFile.Dispose();
        }
    }

}

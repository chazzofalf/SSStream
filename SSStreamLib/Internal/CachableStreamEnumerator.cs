using System.Collections;

namespace SStreamLib.Internal
{
    public partial class CacheableStreamEnumerable
    {
        internal class CachableStreamEnumerator : IEnumerator<byte>
        {
            public TempFileStream stream;
            private byte? @byte = null;
            private byte[] block;
            private int blockIndex = 0;
            private int blockLength = 0;
            private bool eof = false;
            public CachableStreamEnumerator(TempFileStream stream)
            {
                this.stream = stream;
            }
            public byte Current
            {
                get
                {
                    if (@byte == null)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        return @byte.Value;
                    }
                }
            }


            object IEnumerator.Current
            {
                get
                {
                    if (@byte == null)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        return @byte.Value;
                    }
                }
            }

            public void Dispose()
            {
                stream.Close();
            }

            public bool MoveNext()
            {
                if (eof) { return false; }
                if (block == null || blockIndex == block.Length)
                {
                    block = new byte[4096];
                    blockLength = stream.Read(block);
                    if (blockLength == 0)
                    {
                        eof = true;
                        return false;
                    }
                    else
                    {
                        blockIndex = 0;
                    }
                }
                @byte = block[blockIndex++];
                return true;
            }

            public void Reset()
            {
                stream = stream.Reopen();
            }
        }
    }

}

namespace SStreamLib.Internal
{
    class EnumerableStream : Stream
    {
        
        public EnumerableStream(IEnumerable<byte> source)
        {
            this.enumerator = source.GetEnumerator();
        }
        private IEnumerator<byte> enumerator;
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int len = 0;
            for (int i = offset;i< offset + count; i++)
            {
                if (enumerator.MoveNext())
                {
                    buffer[i] = enumerator.Current;
                    len++;
                }
                else
                {
                    break;
                }
            }
            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}

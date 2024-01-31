namespace SStreamLib.Internal
{
    public class FacadeStream : Stream
    {
        private Stream realReader;
        public FacadeStream(Stream realReader)
        {
            this.realReader = realReader;
        }
        
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            throw new IOException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return realReader.Read(buffer, offset, count);
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

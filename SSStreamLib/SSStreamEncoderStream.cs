using SStreamLib.Internal;

namespace SStreamLib
{
    public class SSStreamEncoderStream : FacadeStream
    {
        public SSStreamEncoderStream(Stream stream) : base(new DecoderStream(stream))
        {
        }
    }
}

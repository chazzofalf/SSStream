using SStreamLib.Internal;

namespace SStreamLib
{
    public class SSStreamDecoderStream : FacadeStream
    {
        public SSStreamDecoderStream(Stream stream) : base(new SStreamLib.Internal.DecoderStream(stream))
        {
        }
    }
}

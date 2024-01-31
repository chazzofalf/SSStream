

namespace SStreamLib.Internal
{
    class REDSplicerEncoderStream : EnumerableStream
    {
        public REDSplicerEncoderStream(IEnumerable<byte> carrierSource,IEnumerable<byte> hiddenSource) : base(CreateEncoder(carrierSource,hiddenSource))
        {
        }
        public REDSplicerEncoderStream(IEnumerable<byte> hiddenSource) : base(CreateEncoder(new RandomEnumerable(), hiddenSource))
        {
        }
        public REDSplicerEncoderStream(Stream carrierSourceStream,Stream hiddenSourceStream) : this(new CacheableStreamEnumerable(carrierSourceStream, false), new CacheableStreamEnumerable(hiddenSourceStream, false)) { }
        public REDSplicerEncoderStream(Stream hiddenSourceStream) : this(new RandomEnumerable(), new CacheableStreamEnumerable(hiddenSourceStream, false)) { }

        private static IEnumerable<byte> CreateEncoder(IEnumerable<byte> carrierSource, IEnumerable<byte> hiddenSource)
        {
            var encoder = new REDSplicerEncoder(carrierSource, hiddenSource);
            return encoder.Encode();
        }
    }
}

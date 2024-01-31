

namespace SStreamLib.Internal
{
    class REDSplicerDecoderStream : EnumerableStream

    {
        public REDSplicerDecoderStream(IEnumerable<byte> source) : base(CreateDecoderSource(source))
        {
        }
        public REDSplicerDecoderStream(Stream streamSource) : this(new CacheableStreamEnumerable(streamSource,false)) { }

        private static IEnumerable<byte> CreateDecoderSource(IEnumerable<byte> source)
        {
            var decoder = new REDSplicerDecoder(source);
            return decoder.Decode();
        }
        
    }
}

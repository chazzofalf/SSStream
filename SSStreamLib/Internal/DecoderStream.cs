namespace SStreamLib.Internal
{
    class DecoderStream : EnumerableStream
    {
        public DecoderStream(IEnumerable<byte> data) : base(DecoderForEnumerable(data)) { }
        public DecoderStream(Stream dataStream) : this(new CacheableStreamEnumerable(dataStream)) { }
        private static IEnumerable<byte> DecoderForEnumerable(IEnumerable<byte> input)
        {
            Decoder encoder = new Decoder(input);
            return encoder.Decode();
        }
    }
}

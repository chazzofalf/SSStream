namespace SStreamLib.Internal
{
    class EncoderStream : EnumerableStream
    {
        public EncoderStream(IEnumerable<byte> data) : base(EncoderForEnumerable(data)) { }
        public EncoderStream(Stream dataStream) : this(new CacheableStreamEnumerable(dataStream,false)) { }
        private static IEnumerable<byte> EncoderForEnumerable(IEnumerable<byte> input)
        {
            Encoder encoder = new Encoder(input);
            return encoder.Encode();
        }
    }
}

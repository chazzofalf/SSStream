namespace SStreamLib.Internal
{
   
    internal class REDSplicerDecoder
    {
        private IEnumerable<byte> source;


        public REDSplicerDecoder(IEnumerable<byte> source)
        {
            this.source = source;

        }

        public IEnumerable<byte> Decode()
        {
            var x =
                this.source
                .Chunk(2)
                .Select(x => x.Reverse())
                .Select(x => x.Last())
                .Select(x => x % 2 == 1)
                .Chunk(8)
                .Select(x =>
                {
                    var outx = 0;
                    return x.Select(xx =>
                    {
                        outx *= 2;
                        outx += (xx ? 1 : 0);
                        return (byte)outx;
                    }).ToArray().Last();
                });
            return x;

        }
        
    }
}

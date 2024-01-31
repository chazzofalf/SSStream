namespace SSStreamLib.Tests
{
    [Collection("Sequentially")]
    public class UnitTest1
    {
        private byte[] in_data;
        private byte[] out_data;
        private byte[] dec_data;
        private bool[] done_flag = new bool[] { false};
        
        [Fact]
        public void Test1() // Write Encoded File
        {

            var stream = GetType().Assembly.GetManifestResourceStream("preamble");
            var istream = new MemoryStream();
            
            stream.CopyTo(istream);
            in_data = istream.ToArray();
            istream.Seek(0, SeekOrigin.Begin);
            stream.Close();
            var ostream = new MemoryStream();
            var estream = new SStreamLib.SSStreamEncoderStream(istream);
            estream.CopyTo(ostream);
            out_data = ostream.ToArray();
            done_flag[0] = true;

            var i2stream = new MemoryStream(out_data);

            //stream.CopyTo(istream);
            //in_data = istream.ToArray();
            i2stream.Seek(0, SeekOrigin.Begin);
            //stream.Close();
            var o2stream = new MemoryStream();
            var e2stream = new SStreamLib.SSStreamDecoderStream(i2stream);
            e2stream.CopyTo(o2stream);
            dec_data = o2stream.ToArray();

            Assert.True(in_data.Length == dec_data.Length &&
                !in_data.Zip(dec_data, (src, dest) => src == dest)
                .Where(test => !test).Any());

            
        }
        
    }
}
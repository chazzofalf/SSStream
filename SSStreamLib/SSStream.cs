using SSStreamLib.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSStreamLib
{
    public class SSStream
    {
        public static Stream WrapDecoder(Stream input)
        {
            return new SSSDecoderStream(input);
        }
        public static Stream WrapEncoder(Stream input)
        {
            return new SSSEncoderStream(input);
        }
    }
}

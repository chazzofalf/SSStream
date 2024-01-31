using SStreamLib.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSStreamLib.Internal
{
    internal class SSSDecoderStream : DecoderStream
    {
        public SSSDecoderStream(Stream noise) : base(new REDSplicerDecoderStream(noise))
        {
        }
    }
}

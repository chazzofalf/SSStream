﻿using SStreamLib.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSStreamLib.Internal
{
    internal class SSSEncoderStream : REDSplicerEncoderStream
    {
        public SSSEncoderStream(Stream hiddenDataStream) : base(new EncoderStream(hiddenDataStream))
        {
        }
    }
}

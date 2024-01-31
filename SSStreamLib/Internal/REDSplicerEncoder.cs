using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace SStreamLib.Internal
{
    internal class REDSplicerEncoder
    {
        private IEnumerable<byte> carrierSource;
        private IEnumerable<byte> hiddenSource;
        public REDSplicerEncoder(IEnumerable<byte> carrierSource, IEnumerable<byte> hiddenSource)
        {
            this.carrierSource = carrierSource;
            this.hiddenSource = hiddenSource;
        }
        public IEnumerable<byte> Encode() { 
            var xout = this.carrierSource
                .Chunk(2)
                .SelectMany(x => x.Reverse())
                .SelectMany(x =>
                {
                    var xx = x;
                    return Enumerable.Range(0, 8).Select(_ =>
                    {
                        var outb = xx % 2 == 1;
                        xx /= 2;
                        return outb;
                    }).Reverse();
                })
                .Zip(hiddenSource
                .SelectMany(x =>
                {
                    var xx = x;
                    return Enumerable.Range(0, 8).Select(_ =>
                    {
                        var outb = xx % 2 == 1;
                        xx /= 2;
                        return outb;
                    }).Reverse();
                })
                .SelectMany(x => Enumerable.Repeat((bool?)null,15).Append(x)),
                (carrier_bit,hidden_bit) => hidden_bit != null ? hidden_bit.Value : carrier_bit)
                .Chunk(8)
                .Select(x=>
                {
                    var outb = 0;
                    return x.Select(xx =>
                    {
                        outb *= 2;
                        outb += (xx ? 1 : 0);
                        return (byte)outb;
                    }).ToArray().Last();
                })
                .Chunk(2)
                .SelectMany(x => x.Reverse());
            return xout;
        }
    }
}

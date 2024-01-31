using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SStreamLib.Internal
{
    internal class Encoder
    {
        private IEnumerable<byte> source;
        private IEnumerable<(int Prime, int Index)> p = Enumerable.Empty<int>()
                .Append(127) // EOL 
                .Append(3)
                .Append(7)
                .Append(11)
                .Append(17)
                .Append(23)
                .Append(29)
                .Append(37)
                .Append(41)
                .Append(47)
                .Append(53)
                .Append(59)
                .Append(67)
                .Append(71)
                .Zip(Enumerable.Range(0, 14), (prime, index) => (Prime: prime, Index: index));

        public Encoder(IEnumerable<byte> source)
        {
            this.source = source;
        }
        public Encoder(Stream streamSource) : this(new CacheableStreamEnumerable(streamSource))
        {
            
        }
        public IEnumerable<byte> Encode()
        {
            return Enumerable.Repeat((byte)54, 54).
            Concat(source
            .SelectMany(x => Enumerable.Empty<int>()
            .Append(x / 16)
            .Append(x % 16))
            .SelectMany(x =>
            {
                var b = Enumerable.Empty<bool>();
                var xx = x;
                for (var i = 0; i < 4; i++)
                {
                    b = b.Append(xx % 2 == 1);
                    xx /= 2;
                }
                return b.Reverse();
            })
            .Aggregate((Output: Enumerable.Empty<int>(), Count: 0, Start: true, LastBit: false), (prev, current) =>
            {
                if (prev.Start)
                {
                    var new_output = Enumerable.Empty<int>().Append(current ? 1 : 0);
                    return (Output: prev.Output.Append(current ? 1 : 0), Count: prev.Count + 1, Start: false, LastBit: current);
                }
                else if (prev.LastBit == current)
                {
                    return (prev.Output, Count: prev.Count + 1, Start: false, LastBit: current);
                }
                else
                {
                    return (Output: prev.Output.Append(prev.Count), Count: 1, Start: false, LastBit: current);
                }
            },
            (final_output) =>
            {
                return final_output.Output.Append(final_output.Count);
            })
            .SelectMany(x =>
            {
                if (x < 13)
                {
                    return Enumerable.Empty<int>().Append(x);
                }
                else
                {
                    var times = x / 13;
                    var modulus = x % 13;
                    var o = Enumerable.Empty<int>();
                    for (int i = 0; i < times; i++)
                    {
                        o = o.Append(13);
                    }
                    o = o.Append(modulus);
                    return o;
                }
            })
            .SelectMany(x =>
            {
                if (x > 0 && x < 13)
                {
                    return Enumerable.Empty<int>().Append(x);
                }
                else if (x == 0)
                {
                    return Enumerable.Empty<int>()
                    .Append(13)
                    .Append(1);
                }
                else if (x == 13)
                {
                    return Enumerable.Empty<int>()
                    .Append(13)
                    .Append(13);
                }
                else
                {
                    throw new ArgumentException();
                }
            })
            .Select(x => p.Select(x1 => (Select: x, Element: x1)).Where(px => px.Element.Index == px.Select).Select(x2 => x2.Element.Prime).Single())
            .Append(p.Where(x => x.Index == 0).Select(x => x.Prime).Single())
            .SelectMany(x =>
            {
                var o = Enumerable.Empty<bool>();
                var xx = x;
                for (var i = 0; i < 7; i++)
                {
                    o = o.Append(xx % 2 == 1);
                    xx /= 2;
                }
                return o.Reverse();
            })

            .Aggregate((Output: Enumerable.Empty<byte>(), Number: 0, ByteSize: 0), (prev, current) =>
            {
                var output = prev.Output;
                var number = prev.Number;
                var byte_size = prev.ByteSize;
                number *= 2;
                number += current ? 1 : 0;
                byte_size += 1;
                if (byte_size == 8)
                {
                    output = output.Append((byte)number);
                    byte_size = 0;
                    number = 0;
                }
                return (Output: output, Number: number, ByteSize: byte_size);



            },
            (fin) =>
            {
                var output = fin.Output;
                var number = fin.Number;
                var byte_size = fin.ByteSize;

                if (byte_size > 0)
                {

                    while (byte_size < 8)
                    {
                        number *= 2;
                        byte_size++;
                    }
                    output = output.Append((byte)number);
                    return output;
                }
                else
                { return output; }


            })
        )
            .Concat(Enumerable.Repeat((byte)54, 54))
            .Concat(Enumerable.Repeat((byte)0, 4));
        }
    }
}

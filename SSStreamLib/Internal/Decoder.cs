namespace SStreamLib.Internal
{
    internal class Decoder
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
        public Decoder(IEnumerable<byte> source)
        {
            this.source = source;
        }
        public Decoder(Stream streamSource) : this(new CacheableStreamEnumerable(streamSource))
        {

        }
        public IEnumerable<byte> Decode()
        {
            var inb = source;
            var magic = inb.Take(54).ToArray();
            inb = inb.Skip(54);
            var first_block = inb.Take(4).ToArray();
            inb = inb.Skip(4);

            var magic_good = !magic.Zip(Enumerable.Repeat((byte)54, 54), (src, test) => src == test).Where(x => !x).Any();
            var not_zero = first_block.Zip(Enumerable.Repeat((byte)0, 4), (src, test) => src == test).Where(x => !x).Any();
            if (!magic_good || !not_zero)
            {
                throw new ArgumentException();
            }
            var data = first_block.Concat(inb)
                .SelectMany(x =>
                {
                    var xx = x;
                    var o = Enumerable.Empty<bool>();
                    return Enumerable.Range(0, 8).Aggregate((Output: Enumerable.Empty<bool>(), Value: x), (prev, current) =>
                    {
                        var new_out = prev.Output;
                        var new_value = prev.Value;
                        new_out = new_out.Append(new_value % 2 == 1);
                        new_value /= 2;
                        return (Output: new_out, Value: new_value);
                    },
                    (fin) =>
                    {
                        return fin.Output.Reverse();
                    });
                })
                .Aggregate((Output: Enumerable.Empty<int>(), Value: 0, Chunk_size: 0, NoMore: false), (prev, current) =>
                {
                    var new_out = prev.Output;
                    var new_value = prev.Value;
                    var new_chunk_size = prev.Chunk_size;
                    var new_no_more = prev.NoMore;
                    if (!new_no_more)
                    {
                        new_value *= 2;
                        new_value += (current ? 1 : 0);
                        new_chunk_size++;
                        if (new_chunk_size == 7)
                        {
                            new_out = new_out.Append(new_value);
                            if (new_value == 127)
                            {
                                new_no_more = true;
                            }
                            new_value = 0;
                            new_chunk_size = 0;
                        }

                    }
                    return (Output: new_out, Value: new_value, Chunk_size: new_chunk_size, NoMore: new_no_more);

                },
                (fin) =>
                {
                    return fin.Output;
                })
                .Select(x => p.Select(x1 => (Selection: x, Element: x1)).Where(x2 => x2.Selection == x2.Element.Prime).Select(x3 => x3.Element.Index).Single())
                .Where(x => x != 0)
                .Aggregate((Output: Enumerable.Empty<int>(), Escape: false), (prev, current) =>
                {
                    var new_output = prev.Output;
                    var new_escape = prev.Escape;
                    if (new_escape)
                    {
                        if (current == 1)
                        {
                            new_output = new_output.Append(0);

                        }
                        else if (current == 13)
                        {
                            new_output = new_output.Append(13);

                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        new_escape = false;
                    }
                    else
                    {
                        if (current == 13)
                        {
                            new_escape = true;
                        }
                        else
                        {
                            new_output = new_output.Append(current);
                        }
                    }
                    return (Output: new_output, Escape: new_escape);
                },
                (fin) =>
                {
                    if (fin.Escape)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        return fin.Output;
                    }
                }).
                Aggregate((Output: Enumerable.Empty<int>(), Number: 0), (prev, current) =>
                {
                    var new_output = prev.Output;
                    var new_number = prev.Number;
                    new_number += current;
                    if (current != 13)
                    {
                        new_output = new_output.Append(new_number);
                        new_number = 0;
                    }
                    return (Output: new_output, Number: new_number);
                },
                (fin) =>
                {
                    if (fin.Number != 0)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        return fin.Output;
                    }
                })
                .Aggregate((Output: Enumerable.Empty<bool>(), starting_bit_defined: false, bit: false), (prev, current) =>
                {
                    var new_output = prev.Output;
                    var new_starting_bit_defined = prev.starting_bit_defined;
                    var new_bit = prev.bit;
                    if (!new_starting_bit_defined)
                    {
                        new_bit = current == 1;
                        new_starting_bit_defined = true;
                    }
                    else
                    {
                        new_output = new_output.Concat(Enumerable.Repeat(new_bit, current));
                        new_bit ^= true;
                    }
                    return (Output: new_output, starting_bit_defined: new_starting_bit_defined, bit: new_bit);
                },
                (fin) =>
                {
                    if (!fin.starting_bit_defined)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        return fin.Output;
                    }
                })
                .Aggregate((Output: Enumerable.Empty<byte>(), Number: 0, Byte_Size: 0), (prev, current) =>
                {
                    var new_output = prev.Output;
                    var new_number = prev.Number;
                    var new_byte_size = prev.Byte_Size;
                    new_number *= 2;
                    new_number += (current ? 1 : 0);
                    new_byte_size++;
                    if (new_byte_size == 8)
                    {
                        new_output = new_output.Append((byte)new_number);
                        new_number = 0;
                        new_byte_size = 0;
                    }
                    return (Output: new_output, Number: new_number, Byte_Size: new_byte_size);
                },
                (fin) =>
                {
                    if (fin.Number != 0 || fin.Byte_Size != 0)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        return fin.Output;
                    }
                });
            return data;
        }
    }
}

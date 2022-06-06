using System;
using System.Collections.Generic;
using System.Text;

namespace BitDataParser
{
    public class BitWrangler
    {
    }

    public class BitSet
    {
        public BitSet(byte[] data)
        {
            Bits = new LinkedList<Bit>();
            foreach (var b in data)
            {
                //var str = Convert.ToString(b, 2).PadLeft(8, '0');
                for (int i = 7; i >= 0; i--)
                {
                    var bit = (b & (1 << i)) != 0;
                    Bits.AddLast(new Bit(bit));
                }
            }
        }

        public LinkedList<Bit> Bits { get; set; }

        public void FlipAllBits()
        {
            foreach (var bit in Bits)
            {
                bit.Flip();
            }
        }

        public byte[] ToByteArray()
        {
            var count = Bits.Count;
            int bytecount = (int) Math.Ceiling(count / 8d);
            var bytes = new byte[bytecount];
            int bitIndex = 0;
            foreach (var bit in Bits)
            {
                int byteIndex = bitIndex / 8;
                int bitInByteIndex = bitIndex % 8;
                byte mask = (byte) (128 >> bitInByteIndex);

                if (bit.Value)
                    bytes[byteIndex] |= mask;
                else
                    bytes[byteIndex] &= (byte) ~mask;
                bitIndex++;
            }

            return bytes;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            int tick = 0;
            foreach (var bit in Bits)
            {
                if (tick > 0 && tick % 8 == 0)
                    sb.Append("-");
                if (bit.Value)
                    sb.Append("1");
                else
                    sb.Append("0");

                tick++;
            }

            return sb.ToString();
        }
    }

    public class Bit
    {
        public Bit(bool val)
        {
            Value = val;
        }

        public bool Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.ToString();
        }

        public void Flip()
        {
            Value = !Value;
        }
    }
}
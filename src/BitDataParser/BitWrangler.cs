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
        public BitSet(byte data) : this(new byte[1] { data })
        {            
        }

        public BitSet(byte[] data)
        {
            Bits = new List<Bit>();
            foreach (var b in data)
            {
                //var str = Convert.ToString(b, 2).PadLeft(8, '0');
                for (int i = 7; i >= 0; i--)
                {
                    var bit = (b & (1 << i)) != 0;
                    Bits.Add(new Bit(bit));
                }
            }
        }

        public Bit this[int index]
        {
            get 
            {
                return Bits[index];
            }
        }        

        private List<Bit> Bits { get; set; }

        /// <summary>
        /// Returns field from bit set, 1-8 bits long
        /// </summary>
        /// <param name="pos">The zero based position</param>
        /// <param name="len">The length in bits</param>
        /// <returns>A byte containing the selected bits, padded on left with zero</returns>
        public byte GetField(int pos, int len)
        {
            var lastpos = pos + len - 1;
            byte outbyte = 0x00;

            for(int i = 7; pos != lastpos; i--)
            {
                outbyte = Functions.SetBit(outbyte, i, Bits[lastpos--].Value);
            }
            return outbyte;
        }

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
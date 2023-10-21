using System;

namespace BitDataParser
{
    public static class Crackers
    {
        /// <summary>
        /// Gets a variable out from a position in a byte array
        /// </summary>
        /// <param name="data">The data array</param>
        /// <param name="pos">The zero based position of the first byte</param>
        /// <param name="networkByteOrder">The most significant byte appears first, or "left"</param>
        /// <returns></returns>
        public static ushort CrackUInt16(byte[] data, int pos, bool networkByteOrder = true)
        {
            if(networkByteOrder)
            {
                var two = data[pos];
                var one = data[pos + 1];
                return (ushort)((one << 8) | (two));
            }
            else
            {
                return BitConverter.ToUInt16(data, pos);
            }
            
        }

        /// <summary>
        /// Gets a variable out from a position in a byte array
        /// </summary>
        /// <param name="data">The data array</param>
        /// <param name="pos">The zero based position of the first byte</param>
        /// <param name="networkByteOrder">The most significant byte appears first, or "left"</param>
        /// <returns></returns>
        public static uint CrackUInt32(byte[] data, int pos, bool networkByteOrder = true)
        {
            if (networkByteOrder)
            {
                var four = data[pos];
                var three = data[pos + 1];
                var two = data[pos + 2];
                var one = data[pos + 3];
                return (uint)(one << 24 | (two << 16) | (three << 8) | four);
            }
            else
            {
                return BitConverter.ToUInt32(data, pos);
            }
            
        }

        /// <summary>
        /// Gets a variable out from a position in a byte array
        /// </summary>
        /// <param name="data">The data array</param>
        /// <param name="pos">The zero based position of the first byte</param>
        /// <param name="networkByteOrder">The most significant byte appears first, or "left"</param>
        /// <returns></returns>
        public static ulong CrackUInt64(byte[] data, int pos, bool networkByteOrder = true)
        {
            if (networkByteOrder)
            {
                var arr = new byte[8] { data[pos + 7], data[pos + 6], data[pos + 5], data[pos + 4], data[pos + 3], data[pos + 2], data[pos + 1], data[pos] };
                return BitConverter.ToUInt64(arr);
            }
            else
            {
                return BitConverter.ToUInt64(data, pos);
            }


            
        }
    }
}

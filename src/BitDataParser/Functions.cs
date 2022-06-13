using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BitDataParser
{
    public class Functions
    {
        // filters control characters but allows only properly-formed surrogate sequences
        private static readonly Regex InvalidXMLChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled);

        /// <summary>
        ///     Removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        /// <param name="text">The string to be examine</param>
        /// <returns>A string stripped of invalid chars</returns>
        public static string RemoveInvalidXMLChars(string text)
        {
            return String.IsNullOrEmpty(text) ? "" : InvalidXMLChars.Replace(text, "");
        }

        /// <summary>
        ///     Implodes a dictionary into a single string
        /// </summary>
        /// <param name="dic">The dictionary</param>
        /// <returns></returns>
        public static string MakeCommentString(Dictionary<string, string> dic)
        {
            List<string> comments = dic.Select(variable => $"{variable.Key}: {variable.Value}").ToList();
            string commentstring = String.Join(" ", comments);
            return commentstring;
        }

        /// <summary>
        ///     Implodes a dictionary into a single string
        /// </summary>
        /// <param name="dic">The dictionary</param>
        /// <returns></returns>
        public static string MakeCommentString(Dictionary<string, object> dic)
        {
            List<string> comments = dic.Select(variable => $"{variable.Key}: {variable.Value}").ToList();
            string commentstring = String.Join(" ", comments);
            return commentstring;
        }

        public static uint FieldGetter(byte[] bytes, int startbit, int fieldlength)
        {
            var remain = (startbit - 1) % 8;

            int startbyte;
            if (startbit == 1)
                startbyte = 0;
            else
            {
                startbyte = (int)Math.Floor((startbit - 1) / 8f);
            }

            // the lucky accident, and also to remove edge cases
            if (fieldlength == 8 && remain == 0)
                return bytes[startbyte];

            int endbyte = (int)Math.Floor((startbit - 2 + fieldlength) / 8f);


            var list = new bool[fieldlength];
            var listindex = 0;


            for (int i = startbyte; i <= endbyte; i++)
            {
                var workbyte = bytes[i];

                if (i == startbyte)
                {
                    var boolArray = remain + fieldlength > 7
                        ? ConvertByteToBoolArray(workbyte, remain, 7)
                        : ConvertByteToBoolArray(workbyte, remain, remain + fieldlength - 1);
                    Array.Copy(boolArray, 0, list, listindex, boolArray.Length);
                    listindex += boolArray.Length;
                }
                else if (i == endbyte)
                {
                    var boolArray = ConvertByteToBoolArray(workbyte, 0, fieldlength - listindex - 1);
                    Array.Copy(boolArray, 0, list, listindex, boolArray.Length);
                    listindex += boolArray.Length;
                }
                else
                {
                    var boolArray = ConvertByteToBoolArray(workbyte, 0, 7);
                    Array.Copy(boolArray, 0, list, listindex, boolArray.Length);
                    listindex += boolArray.Length;
                }
            }

            uint exp = 1;
            uint outint = 0;
            for (int i = fieldlength - 1; i >= 0; i--)
            {
                var b = list[i];
                if (b)
                    outint += exp;
                exp = exp + exp;
            }

            return outint;
        }

        private static bool[] ConvertByteToBoolArray(byte b, int startbit, int endbit)
        {
            var bytes = new bool[endbit - startbit + 1];
            int i = 0;

            // while this may look a bit stupid, it is at least idiotproof

            if (startbit == 0)
                bytes[i++] = (b & (1 << 7)) != 0;
            if (endbit >= 1 && startbit <= 1)
                bytes[i++] = (b & (1 << 6)) != 0;
            if (endbit >= 2 && startbit <= 2)
                bytes[i++] = (b & (1 << 5)) != 0;
            if (endbit >= 3 && startbit <= 3)
                bytes[i++] = (b & (1 << 4)) != 0;
            if (endbit >= 4 && startbit <= 4)
                bytes[i++] = (b & (1 << 3)) != 0;
            if (endbit >= 5 && startbit <= 5)
                bytes[i++] = (b & (1 << 2)) != 0;
            if (endbit >= 6 && startbit <= 6)
                bytes[i++] = (b & (1 << 1)) != 0;
            if (endbit == 7)
                bytes[i++] = (b & 1) != 0;

            return bytes;
        }

        /// <summary>
        /// Gets a subarray of bytes out of an array of bytes, starting on the specified bit, in network byte order
        /// </summary>
        /// <param name="bytes">A byte array</param>
        /// <param name="startbit">The 1 based index</param>
        /// <param name="fieldLength">The number of bits in the byte array to extract</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static byte[] SubArrayGetterX(byte[] bytes, int startbit, int fieldLength)
        {
            // determine if a bit is within the area to get
            bool within(int bitpos) => bitpos >= startbit && bitpos < (startbit + fieldLength);

            if (bytes.Length * 8 < startbit)
                throw new ArgumentOutOfRangeException("Startbit is outside the max bounds of the byte array");
            if (fieldLength < 0)
                throw new ArgumentOutOfRangeException("Fieldlength cannot be negative");

            var firstByteNumber = (int)Math.Ceiling(startbit / 8d) - 1;

            // based on some brief benchmarking, the getting lucky concept seems to not be very lucky at all
            //if (startbit-1 % 8 == 0 && fieldLength % 8 == 0)
            //{
            //    // it's our lucky day
            //    var turbobytes = new byte[fieldLength/8];
            //    Array.Copy(bytes, firstByteNumber, turbobytes, 0, turbobytes.Length);
            //    return turbobytes;
            //}


            // if the field length is not divisible into whole bytes, we will add zeroes on the right side 
            var rightpad = fieldLength % 8;
            if (rightpad != 0)
                rightpad = 8 - rightpad;

            // the number of bytes that will be output
            var outbytes = new byte[(int)Math.Ceiling(fieldLength / 8d)];
            if (outbytes.Length * 8 != fieldLength + rightpad)
            {
                throw new ArgumentOutOfRangeException("The function is broken");
            }
            // this array of bools represents the bits in the array
            var outBools = new bool[fieldLength + rightpad];

            var outBoolsIndex = 0;

            var lastByteNumber = (int)Math.Ceiling((startbit + fieldLength - 1) / 8d) - 1;

            for (int bytePos = firstByteNumber; bytePos <= lastByteNumber; bytePos++)
            {
                var workByte = bytes[bytePos];
                var firstBitPos = bytePos * 8 + 1;

                if (bytePos == firstByteNumber || bytePos == lastByteNumber)
                {
                    // if on first or last byte, lets just calmly step through each bit
                    for (int i = 0; i < 8; i++)
                    {
                        if (within(firstBitPos + i))
                            outBools[outBoolsIndex++] = GetBit(workByte, i);
                    }
                }
                else
                {
                    // get the whole byte
                    foreach (var boo in GetByte(workByte))
                        outBools[outBoolsIndex++] = boo;
                }



            }

            for (var index = 0; index < outBools.Length; index++)
            {
                int byteIndex = index / 8;

                if (index % 8 == 0 && (outBools.Length - index) >= 8)
                {
                    // on byte border and we can extract a whole byte, so let's

                    outbytes[byteIndex] = SetByte(outBools, index);
                    index += 7;
                }
                else
                {
                    var outBool = outBools[index];


                    int bitInByteIndex = index % 8;

                    outbytes[byteIndex] = SetBit(outbytes[byteIndex], bitInByteIndex, outBool);
                }

            }


            return outbytes;
        }

        public static byte SetByte(bool[] bools, int startpos)
        {
            byte b = 0x00;
            if (bools[startpos + 0])
                b |= 0b10000000;
            if (bools[startpos + 1])
                b |= 0b01000000;
            if (bools[startpos + 2])
                b |= 0b00100000;
            if (bools[startpos + 3])
                b |= 0b00010000;
            if (bools[startpos + 4])
                b |= 0b00001000;
            if (bools[startpos + 5])
                b |= 0b00000100;
            if (bools[startpos + 6])
                b |= 0b00000010;
            if (bools[startpos + 7])
                b |= 0b00000001;

            return b;
        }

        public static byte SetBit(byte b, int pos, bool value)
        {
            byte mask = (byte)(0x80 >> pos);
            //bool isSet = (bytes[byteIndex] & mask) != 0;
            if (value)
            {
                // set to 1
                b |= (byte)mask;
            }
            else
            {
                // Set to zero
                b &= (byte)~mask;
            }



            return b;
        }

        public static bool GetBit(byte b, int pos)
        {
            byte mask = (byte)(0x80 >> pos);
            bool isSet = (b & mask) != 0;
            return isSet;
        }

        public static bool[] GetByte(byte b)
        {
            var bools = new bool[8];
            bools[0] = (b & 0b10000000) != 0;
            bools[1] = (b & 0b01000000) != 0;
            bools[2] = (b & 0b00100000) != 0;
            bools[3] = (b & 0b00010000) != 0;
            bools[4] = (b & 0b00001000) != 0;
            bools[5] = (b & 0b00000100) != 0;
            bools[6] = (b & 0b00000010) != 0;
            bools[7] = (b & 0b00000001) != 0;
            return bools;
        }

        /// <summary>
        /// Overload to pad the number of output bytes to a specified number
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startbit"></param>
        /// <param name="fieldlength"></param>
        /// <param name="noOfReturnBytes"></param>
        /// <returns></returns>
        public static byte[] SubArrayGetter(byte[] bytes, int startbit, int fieldlength, int noOfReturnBytes)
        {

            var sub = SubArrayGetterX(bytes, startbit, fieldlength);
            if (sub.Length == noOfReturnBytes)
                return sub;
            var bytesOut = new byte[noOfReturnBytes];
            Array.Copy(sub, bytesOut, sub.Length);
            return bytesOut;
        }

        /// <summary>
        /// Overload to read and return all the way to the end
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startbit"></param>
        /// <returns></returns>
        public static byte[] SubArrayGetter(byte[] bytes, int startbit)
        {
            int totalLength = bytes.Length * 8;
            return SubArrayGetterX(bytes, startbit, totalLength - (startbit - 1));
        }

        /// <summary>
        /// Converts unix time to DateTime
        /// </summary>
        /// <param name="seconds">Unix epoc in seconds</param>
        /// <returns></returns>
        public static DateTime UnixEpochToDateTime(uint seconds)
        {
            var unixepoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixepoch.AddSeconds(seconds);
        }
    }
}

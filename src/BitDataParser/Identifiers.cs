using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml.Schema;

namespace BitDataParser
{
    [Serializable]
    public class Identifiers : IEquatable<Identifiers>
    {
        public Identifiers() { }
        public Identifiers(List<int> numeric, IPv4 source = null)
        {
            Numeric = numeric;
            Source = source;
        }

        public Identifiers(int[] numeric, IPv4 source = null)
        {
            Numeric = numeric.ToList();
            Source = source;
        }

        /// <summary>
        /// A simple numeric identifier
        /// </summary>
        public List<int> Numeric { get; set; } = new List<int>();

        public IPv4 Source { get; set; }

        public bool Equals(Identifiers other)
        {
            var checks = new List<bool>();

            
            if(Numeric.Count > 0 || other.Numeric.Count > 0)
            {
                var numericMatch = false;
                if (Numeric.Count > 0 && other.Numeric.Count > 0)
                {
                    var common = Numeric.Intersect(other.Numeric);
                    numericMatch = common.Any();
                }
                    
                checks.Add(numericMatch);
            }

            if(Source != null || other.Source != null)
            {
                var sourceMatch = false;
                if (Source != null && other.Source != null)
                    sourceMatch = Source.Equals(other.Source);
                checks.Add(sourceMatch);
            }

            return checks.TrueForAll(x => x);
        }

        public override string ToString()
        {
            var ret = "";
            if(Numeric.Count > 0) 
            { 
                ret = "["+string.Join(", ", Numeric)+"]"; 
            }
            if(Source != null)
            {
                ret += "["+Source.ToString()+"]";
            }

            return ret;
        }
    }

    [Serializable]
    public class IPv4 : IEquatable<IPv4>
    {
        public byte Val1 { get; set; }
        public byte Val2 { get; set; }
        public byte Val3 { get; set; }
        public byte Val4 { get; set; }

        public override string ToString()
        {
            return $"{Val1}.{Val2}.{Val3}.{Val4}";
        }

        public IPAddress ToIPAddress()
        {
            return new IPAddress(ToByteSpan());
        }

        public ReadOnlySpan<byte> ToByteSpan()
        {
            return new ReadOnlySpan<byte>(new byte[] { Val1, Val2, Val3, Val4 });
        }

        public bool Equals(IPv4 other)
        {
            return Val1 == other.Val1 && Val2 == other.Val2 && Val3 == other.Val3 && Val4 == other.Val4;
        }
    }
}
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BitDataParser
{
    [DataContract]
    public class VariableLengthSettings
    {
        /// <summary>
        /// The Name of the parsed field to find and extract value from
        /// Will use the LAST parsed value that STARTS WITH this name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Multiplier to apply to the extracted value before it is used as length. I.e if the value is referring to number of bytes, this value should be 8
        /// </summary>
        [DataMember]
        public int ScalingFactor { get; set; } = 1;

        /// <summary>
        /// If set, the parsed reference value will be compared to this table to get a Length value (ScalingFactor applied first)
        /// If a LookUpTable was used on the reference value, the underlying value will be used instead!
        /// </summary>
        [DataMember]
        public Dictionary<int, int> LookUpTable { get; set; }
    }
}
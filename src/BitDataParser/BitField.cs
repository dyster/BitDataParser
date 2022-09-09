using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BitDataParser
{
    /// <summary>
    /// Used to parse data out from a stream of bit
    /// </summary>
    [DataContract]
    public class BitField
    {
        /// <summary>
        ///     The name of the bitfield/variable
        /// </summary>
        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        ///     A descriptive comment of the signal/variable/data
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("comment")]
        public string Comment { get; set; }

        /// <summary>
        ///     The length in bits of the field, not used if VariableLengthSettings is set
        /// </summary>
        [DataMember]
        [XmlAttribute("length")]
        public int Length { get; set; }

        /// <summary>
        ///     The type of data that should be parsed out of the bits
        /// </summary>
        [DataMember]
        [XmlAttribute("type")]
        public BitFieldType BitFieldType { get; set; }

        /// <summary>
        /// If set, Value will be substituted by the matching entry
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public LookupTable LookupTable { get; set; }

        /// <summary>
        /// If set, the BitField referenced by its Name property here will be used to determine the length of this Field, instead of the Length Parameter.
        /// If the referenced value cannot be converted to uint, this will result in an exception
        /// A value resulting in 0 will skip the value completely, combined with the LookUpTable in VariableLengthSettings this is useful for using a qualifier to make a variable optional
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public VariableLengthSettings VariableLengthSettings { get; set; }

        /// <summary>
        /// If set, the parsed value will be compared to this and if they match this field will not be added to the parsed dataset (useful for large bitmasks as a bitflag enum)
        /// This will be done before Scaling and LookupTable
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public object SkipIfValue { get; set; }

        /// <summary>
        /// If set, this dataset will be used to parse instead of BitFieldType
        /// Length will be used for number of iterations, important to set Length to 1 (or more) if VariableLength isn't used, otherwise it will be skipped!
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DataSetDefinition NestedDataSet { get; set; }

        /// <summary>
        /// If set, will scale the parsed value by this factor (X * Scaling), value cannot be string!
        /// This will be applied before LookupTable if that is set, but after SkipIfValue
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("scaling")]
        public double Scaling { get; set; }

        /// <summary>
        /// If set, appends this string after all other logic has been performed, this will of course make the end result a string regardless of type
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AppendString { get; set; }

        public override string ToString()
        {
            return $"{Name}: {BitFieldType}[{Length}]";
        }

        /// <summary>
        /// Clones this instance into a new one
        /// </summary>
        /// <returns>The cloned instance</returns>
        public BitField Clone()
        {
            var bitField = new BitField
            {
                Name = Name,
                Comment = Comment,
                Length = Length,
                BitFieldType = BitFieldType,
                LookupTable = LookupTable,
                VariableLengthSettings = VariableLengthSettings,
                SkipIfValue = SkipIfValue,
                NestedDataSet = NestedDataSet
            };
            return bitField;
        }

        /// <summary>
        /// Clones this instance into a new one
        /// </summary>
        /// <param name="newName">The name that the cloned instance shall have</param>
        /// <returns>The cloned instance</returns>
        public BitField Clone(string newName)
        {
            BitField bitField = Clone();
            bitField.Name = newName;
            return bitField;
        }
    }
}
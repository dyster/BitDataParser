using System;
using System.Runtime.Serialization;
using System.Text;

namespace BitDataParser
{
    
    public class ParsedField
    {
        /// <summary>
        /// The value arrived to after all parsing rules have been applied
        /// </summary>
        [DataMember]
        public object Value { get; set; }

        /// <summary>
        /// The actual value extracted from byte array, before any parsing rules are applied
        /// </summary>
        [DataMember]
        public object TrueValue { get; set; }

        /// <summary>
        /// Used by the parser to find the BitField used to parse it 
        /// </summary>
        //[JsonIgnore]
        public BitField UsedBitField { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string Name => UsedBitField.Name;

        //[JsonIgnore]
        public string Comment => UsedBitField.Comment;

        /// <summary>
        /// Creates a ParsedField from a BitField
        /// </summary>
        /// <param name="field">The BitField used for parsing</param>
        /// <param name="val">The parsed value</param>
        /// <returns></returns>
        public static ParsedField Create(BitField field, object val)
        {
            return new ParsedField
            {
                Value = val,
                UsedBitField = field
            };
        }

        public static ParsedField Create(string name, string text)
        {
            return new ParsedField
            {
                Value = text,
                UsedBitField = new BitField() {Name = name}
            };
        }

        public static ParsedField Create(string name, object obj)
        {
            return new ParsedField
            {
                Value = obj,
                UsedBitField = new BitField() { Name = name }
            };
        }

        public static ParsedField CreateError(string text)
        {
            return new ParsedField
            {
                Value = text,
                UsedBitField = new BitField() {Name = "ERROR"}
            };
        }

        public override string ToString()
        {
            return $"{Name}: {Value})";
        }

        public string ToStringExtended()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Name: " + Name);
            sb.AppendLine("Comment: " + Comment);
            sb.AppendLine();
            sb.AppendLine("Parser information");
            sb.AppendLine("AppendString: " + UsedBitField.AppendString);
            sb.AppendLine("BitFieldType: " + UsedBitField.BitFieldType);
            sb.AppendLine("Length: " + UsedBitField.Length);
            sb.AppendLine("Scaling: " + UsedBitField.Scaling);
            if (UsedBitField.VariableLengthSettings == null)
                sb.AppendLine("VariableLengthSettings: Not Used");
            else
            {
                sb.AppendLine("VariableLength Source: " + UsedBitField.VariableLengthSettings.Name);
            }

            sb.AppendLine();
            sb.AppendLine("Value: " + Value);
            sb.AppendLine("True Value: " + TrueValue);

            sb.AppendLine("");
            sb.AppendLine("Warning these bit functions do not work, YET");

            if (Value is string)
            {
                var str = (string) Value;
                char[] charArray = str.ToCharArray();
                sb.Append("String bit inverted: ");
                foreach (var c in charArray)
                {
                    sb.Append(~c);
                }

                sb.Append(Environment.NewLine);

                Array.Reverse(charArray);
                sb.AppendLine("String reversed: " + new string(charArray));
            }
            else
                switch (UsedBitField.BitFieldType)
                {
                    case BitFieldType.Invalid:
                        sb.AppendLine("Invalid BitField type");
                        break;
                    case BitFieldType.Int8:
                        sb.AppendLine("Value bit inverted: " + ~(SByte) Value);
                        sb.AppendLine("TrueValue bit inverted: " + ~(SByte) TrueValue);
                        break;
                    case BitFieldType.Int16:
                        sb.AppendLine("Value bit inverted: " + ~(Int16) Value);
                        sb.AppendLine("TrueValue bit inverted: " + ~(Int16) TrueValue);
                        break;
                    case BitFieldType.Int32:
                        sb.AppendLine("Value bit inverted: " + ~(Int32) Value);
                        sb.AppendLine("TrueValue bit inverted: " + ~(Int32) TrueValue);
                        break;
                    case BitFieldType.Int64:
                        sb.AppendLine("Value bit inverted: " + ~(Int64) Value);
                        sb.AppendLine("TrueValue bit inverted: " + ~(Int64) TrueValue);
                        break;
                    case BitFieldType.UInt8:
                        sb.AppendLine("Value bit inverted: " + ~(byte) Value);
                        sb.AppendLine("TrueValue bit inverted: " + ~(byte) TrueValue);
                        break;
                    case BitFieldType.UInt16:
                        sb.AppendLine("Value bit inverted: " + ~(UInt16) Value);
                        sb.AppendLine("TrueValue bit inverted: " + ~(UInt16) TrueValue);
                        break;
                    case BitFieldType.UInt32:
                        sb.AppendLine("Value bit inverted: " + ~(UInt32) Value);
                        sb.AppendLine("TrueValue bit inverted: " + ~(UInt32) TrueValue);
                        break;
                    case BitFieldType.UInt64:
                        sb.AppendLine("Value bit inverted: " + ~(UInt64) Value);
                        sb.AppendLine("TrueValue bit inverted: " + ~(UInt64) TrueValue);
                        break;
                    case BitFieldType.StringAscii:
                    case BitFieldType.StringUtf8:
                    case BitFieldType.StringLittleEndUtf16:
                    case BitFieldType.StringBigEndUtf16:
                    case BitFieldType.StringLatin:

                        break;
                    case BitFieldType.Float32:
                        break;
                    case BitFieldType.Float64:
                        break;
                    case BitFieldType.Float128:
                        break;
                    case BitFieldType.Bool:
                        break;
                    case BitFieldType.ByteArray:
                        break;
                    case BitFieldType.HexString:
                        break;
                    case BitFieldType.Spare:
                        break;
                    case BitFieldType.UnixEpochUtc:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


            return sb.ToString();
        }
    }

    /// <summary>
    /// This class is used when doing extensive parsing and holds a lot more information
    /// </summary>
    public class ExtensiveParsedField : ParsedField
    {
        /// <summary>
        /// number of bits parsed for this field
        /// </summary>
        public int BitLength { get; set; }

        /// <summary>
        /// Creates an ExtensiveParsedField from a BitField
        /// </summary>
        /// <param name="field">The BitField used for parsing</param>
        /// <param name="val">The parsed value</param>
        /// <returns></returns>
        public new static ExtensiveParsedField Create(BitField field, object val)
        {
            return new ExtensiveParsedField
            {
                Value = val,
                UsedBitField = field
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BitDataParser
{
    /// <summary>
    /// This defines a set of data composed up by different bit fields that can be parsed
    /// </summary>
    [Serializable]
    [DataContract]
    public class DataSetDefinition : IComparable
    {
        private ParsedDataSet _parentSet;

        /// <summary>
        /// A name for this dataset (e.g Subset 27 Header)
        /// </summary>
        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// User comment
        /// </summary>
        [DataMember]
        public string Comment { get; set; }

        /// <summary>
        /// If the dataset is bound to a specific unique id, use this
        /// </summary>
        [DataMember]
        public List<string> Identifiers { get; set; } = new List<string>();

        /// <summary>
        /// A list of BitFields that describes how to parse data
        /// </summary>
        [DataMember]
        public List<BitField> BitFields { get; set; }

        /// <summary>
        /// Setting this to true will flip all bits in the data before processing, i.e 1 will be 0 and 0 will be 1. Don't set this on nested datasets unless only the nested data is inverted
        /// </summary>
        [DataMember]
        [XmlAttribute("invertbits")]
        public bool InvertBits { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return -1;
            var other = (DataSetDefinition) obj;
            return Name.CompareTo(other.Name);
        }

        /// <summary>
        /// Gets the length of all the BitFields combined, does not work if some of them are variable length!
        /// </summary>
        /// <returns>The length in bits</returns>
        public int GetBitLength()
        {
            return BitFields.Where(field => field.VariableLengthSettings == null).Sum(field => field.Length);
        }

        /// <summary>
        /// Overload used internally for nested dataset parsing
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private ParsedDataSet Parse(byte[] data, ParsedDataSet parent, bool extensive)
        {
            _parentSet = parent;
            return Parse(data, extensive);
        }

        /// <summary>
        ///     Parses the provided byte array by way of the bitfields and returns a parsed dataset
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ParsedDataSet Parse(byte[] data, bool extensive = false, int offset = 0)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var dataSet = ParsedDataSet.Create(this);

            int sum = GetBitLength();
            if (sum > data.Length * 8 + offset)
            {
                this.Error = true;
                dataSet.ParsedFields.Add(ParsedField.CreateError("The number of bitfields exceeds the length of the data array"));
                return ReturnMethod();
            }

            if (InvertBits)
            {
                BitSet set = new BitSet(data);
                set.FlipAllBits();
                data = set.ToByteArray();
            }

            // if data is shorter than a byte, shift it to be on the correct edge (left)
            // scrapping this bit, whoever passes data to the function needs to make sure it is correctly shifted to the left
            //if (data.Length == 1 && sum < 8)
            //{
            //    try
            //    {
            //        int shifted = data[0] << (8 - sum);
            //        data[0] = Convert.ToByte(shifted);
            //    }
            //    catch (Exception e)
            //    {
            //        
            //    }
            //}

            var pointer = 1 + offset;
            foreach (var field in BitFields)
            {
                if (field.BitFieldType == BitFieldType.Invalid && field.NestedDataSet == null)
                {
                    throw new Exception("No data defined for this field");
                }

                int length = 0;
                if (field.VariableLengthSettings != null)
                {
                    // get the field where the length is parsed
                    bool Predicate(ParsedField f) => f.Name.StartsWith(field.VariableLengthSettings.Name);

                    ParsedField masterParsedField = null;

                    // check if the field is available
                    if (dataSet.ParsedFields.Any(Predicate))
                    {
                        // we will use the last available field which is closest to the dataset
                        masterParsedField = dataSet.ParsedFields.Last(Predicate);
                    }
                    else
                    {
                        // check parent fields if there is a parent
                        if (_parentSet != null)
                        {
                            if (_parentSet.ParsedFields.Any(Predicate))
                            {
                                // we will use the last available field which is closest to the dataset
                                masterParsedField = _parentSet.ParsedFields.Last(Predicate);
                            }
                            else
                            {
                                this.Error = true;
                                dataSet.ParsedFields.Add(ParsedField.CreateError("The Variable needed for Variable Length has not been parsed: " + field.VariableLengthSettings.Name));
                                // since length is now broken, we have to abort
                                return ReturnMethod();
                            }
                        }
                        else
                        {
                            this.Error = true;
                            dataSet.ParsedFields.Add(ParsedField.CreateError("The Variable needed for Variable Length has not been parsed: " + field.VariableLengthSettings.Name));
                            // since length is now broken, we have to abort
                            return ReturnMethod();
                        }
                    }

                    int masterValue = Convert.ToInt32(masterParsedField.TrueValue);

                    length = masterValue * field.VariableLengthSettings.ScalingFactor;

                    // if there is a LookUpTable on the current value, assign it
                    if (field.VariableLengthSettings.LookUpTable != null)
                    {
                        try
                        {
                            length = field.VariableLengthSettings.LookUpTable[length];
                        }
                        catch(NullReferenceException)
                        {
                            this.Error = true;
                            dataSet.ParsedFields.Add(ParsedField.CreateError("Variable length lookup table did not contain value "+length));

                            // since length is now broken, we have to abort
                            return ReturnMethod();
                        }
                        
                    }
                }
                else
                {
                    length = field.Length;
                }

                if (length == 0)
                    continue;

                object set;

                // parse dataset instead of bitfield
                if (field.NestedDataSet != null)
                {
                    for (int i = 0; i < length; i++)
                    {
                        if (data.Length * 8 < pointer)
                        {
                            this.Error = true;
                            dataSet.ParsedFields.Add(ParsedField.CreateError("There is not enough data to continue parsing"));
                            return ReturnMethod();
                        }

                        // datasetparser will always start at 0, so get the sub array, don't know length so get the whole thing
                        byte[] subData = Functions.SubArrayGetter(data, pointer);
                        ParsedDataSet nestedSet = field.NestedDataSet.Parse(subData, dataSet, extensive);

                        foreach (ParsedField nestedSetParsedField in nestedSet.ParsedFields)
                        {
                            dataSet.ParsedFields.Add(nestedSetParsedField);
                        }

                        pointer += nestedSet.BitsRead;
                    }

                    continue;
                }

                if (data.Length * 8 < pointer || data.Length * 8 < (pointer - 1) + length)
                {
                    this.Error = true;
                    dataSet.ParsedFields.Add(ParsedField.CreateError("There is not enough data to continue parsing"));
                    return ReturnMethod();
                }


                switch (field.BitFieldType)
                {
                    case BitFieldType.Int8:
                        if (length > 8)
                            throw new ArgumentOutOfRangeException("Length has to be at max 8 for this type");
                        //var subArrayGetter = Functions.SubArrayGetter(data, pointer, length);
                        var fieldGetter = Functions.FieldGetter(data, pointer, length);

                        set = unchecked((sbyte) fieldGetter);
                        break;
                    case BitFieldType.Int16:
                        if (length > 16)
                            throw new ArgumentOutOfRangeException("Length has to be at max 16 for this type");
                        //if (length == 16)
                        //    set = BitConverter.ToInt16(
                        //        Functions.SubArrayGetter(data, pointer, length, 2).Reverse().ToArray(), 0);
                        //else
                        //    set = Convert.ToInt16(Functions.FieldGetter(data, pointer, length));
                        
                        set = unchecked((short)Functions.FieldGetter(data, pointer, length));

                        break;
                    case BitFieldType.Int32:
                        if (length != 32)
                            throw new ArgumentOutOfRangeException("Length has to be 32 for this type");
                        //set = BitConverter.ToInt32(
                        //    Functions.SubArrayGetter(data, pointer, length, 4).Reverse().ToArray(), 0);

                        set = unchecked((int)Functions.FieldGetter(data, pointer, length));

                        break;
                    case BitFieldType.Int64:
                        if (length != 64)
                            throw new ArgumentOutOfRangeException("Length has to be 64 for this type");
                        set = BitConverter.ToInt64(
                            Functions.SubArrayGetterX(data, pointer, length).Reverse().ToArray(), 0);
                        break;
                    case BitFieldType.UInt8:
                        if (length > 8)
                            throw new ArgumentOutOfRangeException("Length has to be at max 8 for this type");
                        set = Functions.FieldGetter(data, pointer, length);
                        break;
                    case BitFieldType.UInt16:
                        if (length > 16)
                            throw new ArgumentOutOfRangeException("Length has to be at max 16 for this type");
                        set = (ushort) Functions.FieldGetter(data, pointer, length);
                        break;
                    case BitFieldType.UInt16Reverse:
                        if (length > 16)
                            throw new ArgumentOutOfRangeException("Length has to be at max 16 for this type");
                        var tempU16 = (ushort)Functions.FieldGetter(data, pointer, length);
                        // reverse using Bitconverter before setting
                        set = BitConverter.ToUInt16(BitConverter.GetBytes(tempU16).Reverse().ToArray(), 0);
                        break;
                    case BitFieldType.UInt32:
                        set = Functions.FieldGetter(data, pointer, length);
                        break;
                    case BitFieldType.UInt64:
                        if (length != 64)
                            throw new ArgumentOutOfRangeException("Length has to be 64 for this type");
                        set = BitConverter.ToUInt64(
                            Functions.SubArrayGetterX(data, pointer, length).Reverse().ToArray(), 0);
                        break;
                    case BitFieldType.StringUtf8:
                        set = StringParser(Encoding.UTF8, Functions.SubArrayGetterX(data, pointer, length));
                        break;
                    case BitFieldType.StringAscii:
                        set = StringParser(Encoding.ASCII, Functions.SubArrayGetterX(data, pointer, length));
                        break;
                    case BitFieldType.StringLittleEndUtf16:
                        set = StringParser(Encoding.Unicode, Functions.SubArrayGetterX(data, pointer, length));
                        break;
                    case BitFieldType.StringBigEndUtf16:
                        set = StringParser(Encoding.BigEndianUnicode,
                            Functions.SubArrayGetterX(data, pointer, length));
                        break;
                    case BitFieldType.StringLatin:
                        set = StringParser(Encoding.GetEncoding("iso-8859-1"),
                            Functions.SubArrayGetterX(data, pointer, length));
                        break;
                    case BitFieldType.Float32:
                        if (length != 32)
                            throw new ArgumentOutOfRangeException("Length has to be 32 for this type");
                        set = BitConverter.ToSingle(
                            Functions.SubArrayGetter(data, pointer, length, 4).Reverse().ToArray(), 0);
                        break;
                    case BitFieldType.Float64:
                        if (length != 64)
                            throw new ArgumentOutOfRangeException("Length has to be 64 for this type");
                        set = BitConverter.ToDouble(
                            Functions.SubArrayGetter(data, pointer, length, 4).Reverse().ToArray(), 0);
                        break;
                    case BitFieldType.Float128:
                        throw new NotImplementedException("Why would you do this to me?");
                        break;
                    case BitFieldType.Bool:
                        set = Functions.FieldGetter(data, pointer, length) > 0;
                        break;
                    case BitFieldType.ByteArray:
                        set = Functions.SubArrayGetterX(data, pointer, length);
                        break;
                    case BitFieldType.Spare:
                        set = null;
                        break;
                    case BitFieldType.HexString:
                        set = BitConverter.ToString(Functions.SubArrayGetterX(data, pointer, length));
                        break;
                    case BitFieldType.UnixEpochUtc:
                        if (length == 32)
                        {
                            var unixepoch = BitConverter.ToUInt32(
                                Functions.SubArrayGetter(data, pointer, length, 4).Reverse().ToArray(), 0);
                            set = Functions.UnixEpochToDateTime(unixepoch);
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("Length has to be 32 for this type");
                        }

                        break;
                    default:
                        set = BitConverter.ToString(Functions.SubArrayGetterX(data, pointer, length));
                        break;
                }

                var truevalue = set;

                if (field.SkipIfValue != null)
                {
                    if (field.SkipIfValue.Equals(set))
                        set = null;
                }

                if (set != null)
                {
                    if (field.Scaling != 0f)
                    {
                        try
                        {
                            set = Convert.ToDouble(set) * field.Scaling;
                        }
                        catch (Exception e)
                        {
                            // TODO do something about this!
                            //Logger.Log("Applying scaling failed", Severity.Warning, e);
                        }
                    }

                    if (field.LookupTable != null && field.LookupTable.Count > 0)
                    {
                        // if the value is not in the table, the original value will be kept instead
                        if (field.LookupTable.ContainsKey(set.ToString()))
                        {
                            try
                            {
                                set = field.LookupTable[set.ToString()];
                            }
                            catch (Exception e)
                            {
                                // TODO do something about this!
                                //Logger.Log("Value lookup failed", Severity.Error, e);
                            }
                        }
                    }

                    if (field.AppendString != null)
                    {
                        set = set + field.AppendString;
                    }

                    if (extensive)
                    {
                        var extensiveParsedField = ExtensiveParsedField.Create(field, set);
                        extensiveParsedField.BitLength = length;
                        extensiveParsedField.TrueValue = truevalue;
                        dataSet.ParsedFields.Add(extensiveParsedField);
                    }
                    else
                    {
                        var parsedField = ParsedField.Create(field, set);
                        parsedField.TrueValue = truevalue;
                        dataSet.ParsedFields.Add(parsedField);
                    }
                }

                pointer += length;
            }

            dataSet.BitsRead = pointer - 1 - offset;

            return ReturnMethod();

            ParsedDataSet ReturnMethod()
            {
                stopwatch.Stop();
                PerformanceInfo.DataSetsCreated++;
                PerformanceInfo.TimeSpent += stopwatch.Elapsed;                
                return dataSet;
            }
        }

        // indicates that some parsing error has occured within the dataset
        [XmlIgnore]
        public bool Error { get; set; }

        [XmlIgnore]
        public PerformanceInfo PerformanceInfo { get; set; } = new PerformanceInfo();

        private static string StringParser(Encoding encoding, byte[] data)
        {
            string trim = encoding.GetString(data).Trim('\0');
            trim = Functions.RemoveInvalidXMLChars(trim);
            return trim.Trim();
        }

        public override string ToString()
        {
            if (Identifiers.Count == 0)
                return Name;
            else
                return Name + " [" + string.Join(",", Identifiers) + "]";
        }

        public string Serialize()
        {
            string output;
            var serializer = new DataContractSerializer(GetType());
            using (var stream = new StringWriter())
            {
                using (var xmlwriter = XmlWriter.Create(stream))
                {
                    serializer.WriteObject(xmlwriter, this);
                }

                output = stream.ToString();
            }

            return output;
        }
    }

    /// <summary>
    /// Holds performance info collected during usage
    /// </summary>
    public class PerformanceInfo
    {
        public int DataSetsCreated { get; internal set; } = 0;

        public TimeSpan TimeSpent { get; internal set; } = new TimeSpan();

        /// <summary>
        /// Calculates the average time spent per dataset
        /// </summary>
        public double TimePerDataset => TimeSpent.TotalMilliseconds / DataSetsCreated;

        public override string ToString()
        {
            return $"DataSets created: {DataSetsCreated}, Total time spent: {TimeSpent}, average per set: {TimePerDataset}ms";
        }

    }
}
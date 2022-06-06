using System.Runtime.Serialization;

namespace BitDataParser
{
    /// <summary>
    /// Used to determine what datatype to extract from the raw data
    /// </summary>
    [DataContract]
    public enum BitFieldType
    {
        /// <summary>
        /// This is to signify that no data has been chosen, for error detection
        /// </summary>
        [EnumMember] Invalid,

        /// <summary>
        ///     sbyte
        /// </summary>
        [EnumMember] Int8,

        /// <summary>
        ///     short
        /// </summary>
        [EnumMember] Int16,

        /// <summary>
        ///     int
        /// </summary>
        [EnumMember] Int32,

        /// <summary>
        ///     long
        /// </summary>
        [EnumMember] Int64,

        /// <summary>
        ///     byte
        /// </summary>
        [EnumMember] UInt8,

        /// <summary>
        ///     ushort
        /// </summary>
        [EnumMember] UInt16,

        /// <summary>
        ///     ushort, read out and then byte reversed
        /// </summary>
        [EnumMember] UInt16Reverse,

        /// <summary>
        ///     uint
        /// </summary>
        [EnumMember] UInt32,

        /// <summary>
        ///     ulong
        /// </summary>
        [EnumMember] UInt64,

        /// <summary>
        ///     Encoding.ASCII
        /// </summary>
        [EnumMember] StringAscii,

        /// <summary>
        ///     Encoding.Utf8
        /// </summary>
        [EnumMember] StringUtf8,

        /// <summary>
        ///     Encoding.Unicode
        /// </summary>
        [EnumMember] StringLittleEndUtf16,

        /// <summary>
        ///     Encoding.BigEndianUnicode
        /// </summary>
        [EnumMember] StringBigEndUtf16,

        /// <summary>
        ///     iso-8859-1 or Latin1
        /// </summary>
        [EnumMember] StringLatin,

        /// <summary>
        ///     float
        /// </summary>
        [EnumMember] Float32,

        /// <summary>
        ///     double
        /// </summary>
        [EnumMember] Float64,

        /// <summary>
        ///     decimal
        /// </summary>
        [EnumMember] Float128,

        /// <summary>
        ///     bool
        /// </summary>
        [EnumMember] Bool,

        /// <summary>
        ///     byte[]
        /// </summary>
        [EnumMember] ByteArray,

        /// <summary>
        /// Value will be saved as a Hex String
        /// </summary>
        [EnumMember] HexString,

        /// <summary>
        ///     Will be skipped completely
        /// </summary>
        [EnumMember] Spare,

        /// <summary>
        /// UTC time as seconds since 01.01.1970, 00:00:00, only supports Uint32
        /// </summary>
        [EnumMember] UnixEpochUtc
    }
}
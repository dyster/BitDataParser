using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BitDataParser
{
    
    public class ParsedDataSet : IComparable
    {
        /// <summary>
        /// The parsed data in a more complex form
        /// </summary>        
        public List<ParsedField> ParsedFields { get; set; } = new List<ParsedField>();

        /// <summary>
        /// Gets the number of bits that was read while parsed
        /// </summary>
        [JsonIgnore]
        public int BitsRead { get; set; }

        public ParsedField this[int i] => ParsedFields[i];

        [JsonIgnore]
        public DataSetDefinition Definition { get; private set; }

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (Definition != null) return Definition.Name;
                return "NULL";
            }
        }

        [JsonIgnore]
        public string Comment => Definition?.Comment;

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            var other = (ParsedDataSet) obj;
            var otherstring = Functions.MakeCommentString(other.GetDataDictionary());
            var thisstring = Functions.MakeCommentString(GetDataDictionary());

            return thisstring.CompareTo(otherstring);
        }

        public ParsedField GetField(string name)
        {
            ParsedField parsedField = ParsedFields.Find(field => field.Name == name);
            return parsedField;
        }

        /// <summary>
        ///     This is a simple representation of the parsed data
        /// </summary>
        public Dictionary<string, object> GetDataDictionary()
        {
            var dictionary = new Dictionary<string, object>();

            foreach (ParsedField parsedField in ParsedFields)
            {
                string newname = parsedField.Name;
                if (dictionary.ContainsKey(newname))
                {
                    int i = 2;
                    while (dictionary.ContainsKey(newname + i))
                    {
                        i++;
                    }

                    newname = parsedField.Name + i;
                }

                dictionary.Add(newname, parsedField.Value);
            }

            return dictionary;
        }

        /// <summary>
        ///     Convenience function to get the dictionary with strings instead of objects
        /// </summary>
        public Dictionary<string, string> GetStringDictionary()
        {
            return GetDataDictionary().ToDictionary(entry => entry.Key, entry => entry.Value.ToString());
        }


        /// <summary>
        /// Creates a ParsedDataSet from the DataSetDefinition used to parse the data
        /// </summary>
        /// <param name="baseDataSet"></param>
        /// <returns></returns>
        public static ParsedDataSet Create(DataSetDefinition baseDataSet)
        {
            return new ParsedDataSet {Definition = baseDataSet};
        }

        /// <summary>
        /// Creates a mock dataset with one parsed field, useful for replacing parsed data with an error message
        /// </summary>
        /// <param name="fieldname">The name for the single field</param>
        /// <param name="fieldvalue">The value of the single field</param>
        /// <returns></returns>
        public static ParsedDataSet CreateError(string text)
        {
            return new ParsedDataSet {ParsedFields = new List<ParsedField> {ParsedField.CreateError(text)}};
        }

        public override string ToString()
        {
            return Functions.MakeCommentString(GetDataDictionary());
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null)
        //        return false;
        //
        //    var other = (ParsedDataSet) obj;
        //
        //    if (this.ParsedFields.Count != other.ParsedFields.Count)
        //        return false;
        //    
        //
        //    for (var index = 0; index < ParsedFields.Count; index++)
        //    {
        //        var parsedField = ParsedFields[index];
        //        if (other.ParsedFields[index] == null)
        //            return false;
        //
        //        if (parsedField.Name != other.ParsedFields[index].Name)
        //            return false;
        //        if (!parsedField.Value.Equals(other.ParsedFields[index].Value))
        //            return false;
        //    }
        //
        //    return true;
        //}

        public bool Equals(object obj, List<string> ignores)
        {
            if (obj == null)
                return false;

            var other = (ParsedDataSet) obj;

            if (ParsedFields.Count != other.ParsedFields.Count)
                return false;


            for (var index = 0; index < ParsedFields.Count; index++)
            {
                var parsedField = ParsedFields[index];
                if (ignores.Contains(parsedField.Name))
                    continue;

                if (other.ParsedFields[index] == null)
                    return false;

                if (parsedField.Name != other.ParsedFields[index].Name)
                    return false;
                if (!parsedField.Value.Equals(other.ParsedFields[index].Value))
                    return false;
            }

            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace BitDataParser
{
    /// <summary>
    /// A base collection for DataSets so they can be described and found in the assembly, as well as serialized neatly
    /// </summary>
    [Serializable]
    [DataContract]
    public class DataSetCollection : makeSerializable
    {
        [DataMember] public string Name { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public List<DataSetDefinition> DataSets { get; set; } = new List<DataSetDefinition>();

        public DataSetDefinition FindByIdentifier(Identifiers ident)
        {
            foreach (var d in DataSets)
            {
                if (d.Identifiers.Equals(ident))
                {
                    return d;
                }
            }

            return null;
        }        
    }
}
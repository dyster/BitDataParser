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
    public class DataSetCollection
    {
        [DataMember] public string Name { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public List<DataSetDefinition> DataSets { get; set; } = new List<DataSetDefinition>();

        public DataSetDefinition FindByIdentifier(string ident)
        {
            foreach (var d in DataSets)
            {
                if (d.Identifiers.Contains(ident))
                {
                    return d;
                }
            }

            return null;
        }

        public void Serialize(string path)
        {
            DataContractSerializer formatter = new DataContractSerializer(typeof(DataSetCollection),
                new DataContractSerializerSettings()
                    {MaxItemsInObjectGraph = Int32.MaxValue, KnownTypes = new List<Type>() {GetType()}});
            using (var xmlWriter = XmlWriter.Create(File.Create(path), new XmlWriterSettings() {Indent = true}))
            {
                formatter.WriteObject(xmlWriter, this);
            }
        }
    }
}
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;

namespace BitDataParser
{
    [Serializable]
    public abstract class makeSerializable
    {
        public void SerializeDataContract(string filePath, bool indent = true)
        {
            DataContractSerializer serializer = new DataContractSerializer(this.GetType(), new DataContractSerializerSettings()
            { MaxItemsInObjectGraph = Int32.MaxValue });
            using (var file = new FileStream(filePath, FileMode.Create))
            {
                var xmlwriter = XmlWriter.Create(file, new XmlWriterSettings() { Indent = indent });
                serializer.WriteObject(xmlwriter, this);
            }
        }

        public void SerializeXml(string filePath, bool indent = true)
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            using (var file = new FileStream(filePath, FileMode.Create))
            {
                var xmlwriter = XmlWriter.Create(file, new XmlWriterSettings() { Indent = indent });
                serializer.Serialize(xmlwriter, this);
            }
        }

        public void SerializeJson(string filePath, bool indent = true)
        {
            using FileStream fileStream = File.Create(filePath);
            JsonSerializer.Serialize(fileStream, this, this.GetType(), new JsonSerializerOptions { WriteIndented = indent, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals });

        }
    }
}

using System.Xml;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Extensions;

public static class XmlWriterExtensions
{
    public static void WriteElement<T>(this XmlWriter writer, string elementName, T value)
    {
        writer.WriteStartElement(elementName);
        writer.WriteValue(value);
        writer.WriteEndElement();
    }

    public static void WriteNullableElement<T>(this XmlWriter writer, string elementName, T? value) where T : struct
    {
        writer.WriteStartElement(elementName);
        writer.WriteValue(value?.ToString() ?? string.Empty);
        writer.WriteEndElement();
    }

    public static void WriteAttribute<T>(this XmlWriter writer, string attributeName, T value)
    {
        writer.WriteAttributeString(attributeName, value?.ToString());
    }

    public static void WriteCollection<T>(this XmlWriter writer,
        string collectionName, string itemName, IEnumerable<T> collection)
        where T : IXmlSerializable
    {
        writer.WriteStartElement(collectionName);
        foreach (var item in collection)
        {
            writer.WriteStartElement(itemName);
            item.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }
}
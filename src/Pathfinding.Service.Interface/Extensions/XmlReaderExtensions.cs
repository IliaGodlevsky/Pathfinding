using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Extensions
{
    public static class XmlReaderExtensions
    {
        public static T ReadElement<T>(this XmlReader reader, string elementName)
        {
            return reader.Read(elementName,
                content => (T)Convert.ChangeType(content, typeof(T), CultureInfo.InvariantCulture));
        }

        public static T ReadEnumElement<T>(this XmlReader reader, string elementName)
            where T : Enum
        {
            return reader.Read(elementName, content => (T)Enum.Parse(typeof(T), content));
        }

        private static T Read<T>(this XmlReader reader, string elementName, Func<string, T> converter)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == elementName
               || reader.ReadToFollowing(elementName))
            {
                var content = reader.ReadElementContentAsString();
                return converter(content);
            }

            return default;
        }

        public static T ReadAttribute<T>(this XmlReader reader, string attributeName)
        {
            var value = reader.GetAttribute(attributeName);
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public static T ReadEnumAttribute<T>(this XmlReader reader, string attributeName)
            where T : struct, Enum
        {
            var value = reader.GetAttribute(attributeName)!;
            return Enum.Parse<T>(value);
        }

        public static List<T> ReadCollection<T>(this XmlReader reader, string collectionName, string itemName)
            where T : IXmlSerializable, new()
        {
            var items = new List<T>();
            if (reader.NodeType == XmlNodeType.Element && reader.Name == collectionName)
            {
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                    return items;
                }

                reader.ReadStartElement(collectionName);

                while (reader.NodeType != XmlNodeType.EndElement || reader.Name != collectionName)
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == itemName)
                    {
                        var item = new T();
                        item.ReadXml(reader);
                        items.Add(item);
                    }
                    else
                    {
                        reader.Read();
                    }
                }

                reader.ReadEndElement();
            }

            return items;
        }

        public static T? ReadNullableElement<T>(this XmlReader reader, string elementName)
            where T : struct
        {
            return reader.Read(elementName,
                content => string.IsNullOrEmpty(content) ? default(T?) : (T)Convert.ChangeType(content, typeof(T)));
        }

        public static T? ReadNullableEnum<T>(this XmlReader reader, string elementName) where T : struct, Enum
        {
            return reader.Read(elementName,
                content => string.IsNullOrEmpty(content) ? default(T?) : (T)Enum.Parse(typeof(T), content));
        }
    }
}

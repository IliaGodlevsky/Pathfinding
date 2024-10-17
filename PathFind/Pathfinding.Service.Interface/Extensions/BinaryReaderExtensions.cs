﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Pathfinding.Service.Interface.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static T ReadSerializable<T>(this BinaryReader reader)
            where T : IBinarySerializable, new()
        {
            T item = new();
            item.Deserialize(reader);
            return item;
        }

        public static IReadOnlyCollection<T> ReadSerializableArray<T>(this BinaryReader reader)
            where T : IBinarySerializable, new()
        {
            int count = reader.ReadInt32();
            var list = new List<T>(count);
            while (count-- > 0)
            {
                list.Add(reader.ReadSerializable<T>());
            }
            return list.AsReadOnly();
        }

        public static string ReadNullableString(this BinaryReader reader)
        {
            bool isNull = reader.ReadBoolean();
            return isNull ? string.Empty : reader.ReadString();
        }

        public static TimeSpan ReadTimeSpan(this BinaryReader reader)
        {
            return TimeSpan.FromMilliseconds(reader.ReadDouble());
        }



        public static IReadOnlyList<int> ReadArray(this BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var array = new List<int>(count);
            while (count-- > 0) array.Add(reader.ReadInt32());
            return array.AsReadOnly();
        }
    }
}

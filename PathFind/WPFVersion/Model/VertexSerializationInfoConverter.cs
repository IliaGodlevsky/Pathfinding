﻿using GraphLib.Graphs.Serialization.Factories.Interfaces;
using GraphLib.Info;
using GraphLib.Vertex.Interface;

namespace WPFVersion.Model
{
    internal class VertexSerializationInfoConverter : IVertexSerializationInfoConverter
    {
        public IVertex ConvertFrom(VertexSerializationInfo info)
        {
            return new Vertex(info);
        }
    }
}

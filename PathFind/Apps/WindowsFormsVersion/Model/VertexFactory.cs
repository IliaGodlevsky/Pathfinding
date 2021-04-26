﻿using GraphLib.Interfaces;
using GraphLib.Interfaces.Factories;

namespace WindowsFormsVersion.Model
{
    internal sealed class VertexFactory : IVertexFactory
    {
        public IVertex CreateVertex(ICoordinateRadar coordinateRadar, ICoordinate coordinate)
        {
            return new Vertex(coordinateRadar, coordinate);
        }
    }
}

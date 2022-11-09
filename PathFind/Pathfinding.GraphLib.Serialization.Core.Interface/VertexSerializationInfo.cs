﻿using Pathfinding.GraphLib.Core.Interface;
using Shared.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.GraphLib.Serialization.Core.Interface
{
    public sealed class VertexSerializationInfo
    {
        private sealed class Neighborhood : INeighborhood
        {
            private readonly IReadOnlyCollection<ICoordinate> coordinates;

            public int Count => coordinates.Count;

            public Neighborhood(IReadOnlyCollection<IVertex> vertices)
            {
                this.coordinates = vertices
                    .Select(vertex => vertex.Position)
                    .ToReadOnly();
            }

            public Neighborhood(IReadOnlyCollection<ICoordinate> coordinates)
            {
                this.coordinates = coordinates;
            }

            public IEnumerator<ICoordinate> GetEnumerator()
            {
                return coordinates.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public bool IsObstacle { get; }

        public IVertexCost Cost { get; }

        public ICoordinate Position { get; }

        public INeighborhood Neighbourhood { get; }

        public VertexSerializationInfo(IVertex vertex)
            : this(vertex.IsObstacle, vertex.Cost, vertex.Position, new Neighborhood(vertex.Neighbours))
        {

        }

        public VertexSerializationInfo(bool isObstacle, IVertexCost cost,
            ICoordinate position, IReadOnlyCollection<ICoordinate> neighborhood)
        {
            IsObstacle = isObstacle;
            Cost = cost;
            Position = position;
            Neighbourhood = new Neighborhood(neighborhood);
        }
    }
}
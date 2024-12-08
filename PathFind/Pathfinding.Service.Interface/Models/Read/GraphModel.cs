﻿using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using System;
using System.Collections.Generic;

namespace Pathfinding.Service.Interface.Models.Read
{
    public record class GraphModel<T> : IGraphLayersInfo
        where T : IVertex
    {
        public static readonly GraphModel<T> Empty = new ()
        {
            Id = 0,
            Name = string.Empty,
            Vertices = Array.Empty<T>(),
            DimensionSizes = Array.Empty<int>()
        };

        public int Id { get; set; }

        public string Name { get; set; }

        public SmoothLevels SmoothLevel { get; set; }

        public Neighborhoods Neighborhood { get; set; }

        public GraphStatuses Status { get; set; }

        public IReadOnlyCollection<T> Vertices { get; set; }

        public IReadOnlyList<int> DimensionSizes { get; set; }
    }
}

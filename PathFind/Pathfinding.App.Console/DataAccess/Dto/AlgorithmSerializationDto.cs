﻿using Pathfinding.App.Console.Model.Notes;
using Pathfinding.GraphLib.Core.Interface;
using System.Collections.Generic;

namespace Pathfinding.App.Console.DataAccess.Dto
{
    internal class AlgorithmSerializationDto
    {
        public Statistics Statistics { get; set; }

        public IReadOnlyCollection<ICoordinate> Path { get; set; }

        public IReadOnlyCollection<ICoordinate> Range { get; set; }

        public IReadOnlyCollection<ICoordinate> Visited { get; set; }

        public IReadOnlyCollection<ICoordinate> Obstacles { get; set; }

        public IReadOnlyCollection<int> Costs { get; set; }
    }
}

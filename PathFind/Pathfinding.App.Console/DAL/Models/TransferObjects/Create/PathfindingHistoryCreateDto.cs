﻿using Pathfinding.App.Console.Model;
using Pathfinding.GraphLib.Core.Interface;
using System.Collections.Generic;

namespace Pathfinding.App.Console.DAL.Models.TransferObjects.Create
{
    internal class PathfindingHistoryCreateDto<T>
        where T : IVertex
    {
        public GraphCreateDto<T> Graph { get; set; }

        public IReadOnlyCollection<AlgorithmRunHistoryCreateDto> Algorithms { get; set; }

        public IReadOnlyCollection<ICoordinate> Range { get; set; }
    }
}

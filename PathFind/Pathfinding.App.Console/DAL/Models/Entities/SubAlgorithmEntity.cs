﻿namespace Pathfinding.App.Console.DAL.Models.Entities
{
    internal class SubAlgorithmEntity
    {
        public int Id { get; set; }

        public int AlgorithmId { get; set; }

        public byte[] Visited { get; set; }

        public byte[] Path { get; set; }
    }
}

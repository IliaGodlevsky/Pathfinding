﻿using Pathfinding.App.Console.DAL.Interface;
using Pathfinding.App.Console.Interface;
using Pathfinding.App.Console.Localization;
using Pathfinding.App.Console.MenuItems.MenuItemPriority;
using Pathfinding.App.Console.Model;
using Pathfinding.GraphLib.Core.Interface.Extensions;
using Pathfinding.GraphLib.Core.Modules.Interface;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.App.Console.MenuItems.PathfindingRangeMenuItems
{
    [HighestPriority]
    internal sealed class EnterPathfindingRangeMenuItem : NavigateThroughVerticesMenuItem
    {
        private readonly IPathfindingRangeBuilder<Vertex> builder;
        private readonly VertexActions rangeActions;

        public EnterPathfindingRangeMenuItem(VertexActions actions,
            IInput<ConsoleKey> keyInput,
            IPathfindingRangeBuilder<Vertex> builder,
            IService service)
            : base(keyInput, service)
        {
            this.rangeActions = actions;
            this.builder = builder;
        }

        public override bool CanBeExecuted()
        {
            return graph.Graph.GetNumberOfNotIsolatedVertices() > 1;
        }

        public override void Execute()
        {
            base.Execute();

            var currentRange = service.GetRange(graph.Id)
                .Select((x, i) => (Order: i, Coordinate: x))
                .ToDictionary(x => x.Coordinate, x => x.Order);
            var newRange = builder.Range.GetCoordinates()
                .Select((x, i) => (Order: i, Coordinate: x))
                .ToDictionary(x => x.Coordinate, x => x.Order);

            var added = new List<(int Order, Vertex Vertex)>();
            var updated = new List<(int Order, Vertex Vertex)>();

            foreach (var item in newRange)
            {
                var vertex = graph.Graph.Get(item.Key);
                var value = (item.Value, vertex);
                if (!currentRange.TryGetValue(item.Key, out var order))
                {
                    added.Add(value);
                }
                else if (item.Value != order)
                {
                    updated.Add(value);
                }
            }

            var deleted = currentRange.Where(x => !newRange.ContainsKey(x.Key))
                .Select(x => graph.Graph.Get(x.Key))
                .ToReadOnly();

            service.AddRange(added.ToArray(), graph.Id);
            service.UpdateRange(updated.ToArray(), graph.Id);
            service.RemoveRange(deleted, graph.Id);
        }

        public override string ToString()
        {
            return Languages.EnterPathfindingRange;
        }

        protected override VertexActions GetActions()
        {
            return rangeActions;
        }
    }
}

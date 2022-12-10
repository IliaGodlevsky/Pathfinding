﻿using GalaSoft.MvvmLight.Messaging;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Model;
using Pathfinding.GraphLib.Core.Interface.Extensions;
using Pathfinding.GraphLib.Core.Realizations.Graphs;
using Pathfinding.GraphLib.Factory.Interface;
using Shared.Random;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.App.Console.MenuItems.GraphMenuItems
{
    internal sealed class ResizeGraphMenuItem : GraphCreatingMenuItem
    {
        private Graph2D<Vertex> graph = Graph2D<Vertex>.Empty;

        public override int Order => 2;

        public ResizeGraphMenuItem(IMessenger messenger, IRandom random, 
            IVertexCostFactory costFactory, INeighborhoodFactory neighborhoodFactory)
            : base(messenger, random, costFactory,neighborhoodFactory)
        {
            this.messenger.Register<GraphCreatedMessage>(this, OnGraphCreated);
        }

        private void OnGraphCreated(GraphCreatedMessage msg)
        {
            graph = msg.Graph;
        }     

        public override bool CanBeExecuted()
        {
            return base.CanBeExecuted()
                && (graph.Width != width || graph.Length != length)
                && graph != Graph2D<Vertex>.Empty;
        }

        protected override IReadOnlyCollection<ILayer<Graph2D<Vertex>, Vertex>> CreateLayers()
        {
            return base.CreateLayers()
                .Append(new GraphLayer((Graph2D<Vertex>)graph.Clone()))
                .ToArray();           
        }

        public override string ToString()
        {
            return "Resize graph";
        }
    }
}

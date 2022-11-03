﻿using Common.Extensions.EnumerableExtensions;
using GraphLib.Base;
using GraphLib.Extensions;
using GraphLib.Interfaces;
using GraphLib.Interfaces.Factories;
using GraphLib.Serialization.Exceptions;
using GraphLib.Serialization.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace GraphLib.Serialization.Serializers
{
    public abstract class GraphSerializer<TGraph, TVertex> : IGraphSerializer<TGraph, TVertex>
        where TVertex : IVertex
        where TGraph : IGraph<TVertex>
    {
        private readonly IVertexFromInfoFactory<TVertex> vertexFactory;
        private readonly IGraphFactory<TGraph, TVertex> graphFactory;
        private readonly IVertexCostFactory costFactory;
        private readonly ICoordinateFactory coordinateFactory;

        public GraphSerializer(IVertexFromInfoFactory<TVertex> converter,
            IGraphFactory<TGraph, TVertex> graphFactory,
            IVertexCostFactory costFactory,
            ICoordinateFactory coordinateFactory)
        {
            this.vertexFactory = converter;
            this.graphFactory = graphFactory;
            this.costFactory = costFactory;
            this.coordinateFactory = coordinateFactory;
        }

        public TGraph LoadGraph(Stream stream)
        {
            try
            {
                var graphInfo = LoadGraphInternal(stream, costFactory, coordinateFactory);
                var vertices = graphInfo.VerticesInfo.Select(vertexFactory.CreateFrom).ToReadOnly();
                var graph = graphFactory.CreateGraph(vertices, graphInfo.DimensionsSizes);
                graph.CostRange = graphInfo.CostRange;
                graphInfo.VerticesInfo.Zip(graph, (info, vertex) => (Vertex: vertex, Info: info))
                    .ForEach(item => SetNeighbourhood(item, (IGraph<IVertex>)graph));
                return graph;
            }
            catch (Exception ex)
            {
                throw new CantSerializeGraphException(ex.Message, ex);
            }
        }

        public void SaveGraph(IGraph<IVertex> graph, Stream stream)
        {
            try
            {
                SaveGraphInternal(graph, stream);
            }
            catch (Exception ex)
            {
                throw new CantSerializeGraphException(ex.Message, ex);
            }
        }

        protected abstract GraphSerializationInfo LoadGraphInternal(Stream stream,
            IVertexCostFactory costFactory, ICoordinateFactory coordinateFactory);

        protected abstract void SaveGraphInternal(IGraph<IVertex> graph, Stream stream);

        private void SetNeighbourhood((TVertex Vertex, VertexSerializationInfo Info) info, IGraph<IVertex> graph)
        {
            info.Vertex.Neighbours = info.Info.Neighbourhood.GetNeighboursWithinGraph(graph);
        }
    }
}

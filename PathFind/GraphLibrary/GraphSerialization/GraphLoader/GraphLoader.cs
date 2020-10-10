﻿using GraphLibrary.Coordinates;
using GraphLibrary.DTO;
using GraphLibrary.Graphs;
using GraphLibrary.Graphs.Interface;
using GraphLibrary.GraphSerialization.GraphLoader.Interface;
using GraphLibrary.Vertex.Interface;
using GraphLibrary.VertexConnecting;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace GraphLibrary.GraphSerialization.GraphLoader
{
    /// <summary>
    /// Deserializes graph using BinaryFormatter class
    /// </summary>
    public class GraphLoader : IGraphLoader
    {
        public event Action<string> OnBadLoad;

        public IGraph LoadGraph(string path, Func<VertexDto, IVertex> converter)
        {
            var formatter = new BinaryFormatter();
            try
            {
                using (var stream = new FileStream(path, FileMode.Open))
                { 
                    var verticesDto = (VertexDtoContainer2D)formatter.Deserialize(stream);
                    graph = GetGraphFromDto(verticesDto, converter);
                }
            }
            catch (Exception ex)
            {
                OnBadLoad?.Invoke(ex.Message);
            }
            return graph;
        }

        private IGraph GetGraphFromDto(VertexDtoContainer2D verticesDto, Func<VertexDto, IVertex> dtoConverter)
        {
            graph = new Graph(verticesDto.Width, verticesDto.Height);
            for (int i = 0; i < verticesDto.Width; i++)
            {
                for (int j = 0; j < verticesDto.Height; j++)
                {
                    var indices = new Coordinate2D(i, j);
                    var index = i * verticesDto.Height + j;
                    graph[indices] = dtoConverter(verticesDto.ElementAt(index));
                    graph[indices].Position = indices;
                }
            }            
            VertexConnector.ConnectVertices(graph);
            return graph;
        }

        private IGraph graph = NullGraph.Instance;
    }
}

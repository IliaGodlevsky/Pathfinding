﻿using GraphLib.EventHolder.Interface;
using GraphLib.Extensions;
using GraphLib.GraphField;
using GraphLib.GraphFieldCreating;
using GraphLib.Graphs;
using GraphLib.Graphs.Abstractions;
using GraphLib.Graphs.Factories.Interfaces;
using GraphLib.Graphs.Serialization.Interfaces;
using GraphViewModel.Interfaces;
using GraphViewModel.Resources;
using System.IO;

namespace GraphViewModel
{
    public abstract class MainModel : IMainModel
    {
        public virtual string GraphParametres { get; set; }

        public virtual string PathFindingStatistics { get; set; }

        public virtual IGraphField GraphField { get; set; }

        public virtual IGraph Graph { get; protected set; }


        public MainModel(BaseGraphFieldFactory fieldFactory,
            IVertexEventHolder eventHolder,
            IGraphSerializer graphSerializer,
            IGraphFiller graphFiller)
        {
            this.eventHolder = eventHolder;
            serializer = graphSerializer;
            this.fieldFactory = fieldFactory;
            this.graphFiller = graphFiller;

            Graph = new NullGraph();
            graphParamFormat = ViewModelResources.GraphParametresFormat;
        }

        public virtual void SaveGraph()
        {
            var savePath = GetSavingPath();
            try
            {
                using (var stream = new FileStream(savePath, FileMode.OpenOrCreate))
                {
                    serializer.SaveGraph(Graph, stream);
                }
            }
            catch { }
        }

        public virtual void LoadGraph()
        {
            var loadPath = GetLoadingPath();
            try
            {
                using (var stream = new FileStream(loadPath, FileMode.Open))
                {
                    var newGraph = serializer.LoadGraph(stream);
                    ConnectNewGraph(newGraph);
                }
            }
            catch { }
        }

        public virtual void ClearGraph()
        {
            Graph.Refresh();
            PathFindingStatistics = string.Empty;
            GraphParametres = Graph.GetFormattedData(graphParamFormat);
        }

        public void ConnectNewGraph(IGraph graph)
        {
            if (!graph.IsDefault)
            {
                Graph = graph;
                GraphField = fieldFactory.CreateGraphField(Graph);
                eventHolder.Graph = Graph;
                eventHolder.SubscribeVertices();
                GraphParametres = Graph.GetFormattedData(graphParamFormat);
                PathFindingStatistics = string.Empty;
            }
        }

        protected string graphParamFormat;
        protected readonly IGraphFiller graphFiller;
        private readonly IVertexEventHolder eventHolder;
        private readonly IGraphSerializer serializer;
        private readonly BaseGraphFieldFactory fieldFactory;

        public abstract void FindPath();

        public abstract void CreateNewGraph();

        protected abstract string GetSavingPath();

        protected abstract string GetLoadingPath();
    }
}

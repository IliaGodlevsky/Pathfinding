using Pathfinding.App.Console.Model;
using Pathfinding.Service.Interface.Models.Read;
using System.Reactive;

namespace Pathfinding.App.Console.Messages.ViewModel
{
    internal sealed class AsyncGraphActivatedMessage(GraphModel<GraphVertexModel> graph) : IAsyncMessage<Unit>
    {
        public GraphModel<GraphVertexModel> Graph { get; } = graph;

        public Action<Unit> Signal { get; set; } = unit => throw new InvalidOperationException();
    }
}

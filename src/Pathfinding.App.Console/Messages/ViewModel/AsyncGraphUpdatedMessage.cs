using Pathfinding.Service.Interface.Models.Read;
using System.Reactive;

namespace Pathfinding.App.Console.Messages.ViewModel
{
    internal sealed class AsyncGraphUpdatedMessage(GraphInformationModel model) : IAsyncMessage<Unit>
    {
        public GraphInformationModel Model { get; } = model;

        public Action<Unit> Signal { get; set; } = unit => throw new InvalidOperationException();
    }
}

using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphsCreatedMessage(GraphInfoModel[] models)
    : ValueChangedMessage<GraphInfoModel[]>(models)
{
    public GraphsCreatedMessage(GraphInfoModel model) : this([model])
    {

    }
}
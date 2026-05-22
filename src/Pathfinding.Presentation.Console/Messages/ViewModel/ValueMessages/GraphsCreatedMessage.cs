using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphsCreatedMessage(GraphInfoModel[] models)
    : ValueChangedMessage<GraphInfoModel[]>(models)
{
    public GraphsCreatedMessage(GraphInfoModel model) : this([model])
    {

    }
}
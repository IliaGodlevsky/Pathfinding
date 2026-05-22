using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphsSelectedMessage(GraphInfoModel[] graphs)
    : ValueChangedMessage<GraphInfoModel[]>(graphs);
using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphActivatedMessage(ActivatedGraphModel models) 
    : ValueChangedMessage<ActivatedGraphModel>(models);
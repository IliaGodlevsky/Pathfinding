using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class AsyncGraphActivatedMessage(ActivatedGraphModel model) 
    : AsyncValueChangedMessage<ActivatedGraphModel>(model);
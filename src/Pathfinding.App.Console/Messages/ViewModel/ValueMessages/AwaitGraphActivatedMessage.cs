using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class AwaitGraphActivatedMessage(ActivatedGraphModel model)
    : AwaitValueChangedMessage<ActivatedGraphModel>(model);
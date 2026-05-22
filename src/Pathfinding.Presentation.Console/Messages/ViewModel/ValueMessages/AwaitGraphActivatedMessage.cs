using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal sealed class AwaitGraphActivatedMessage(ActivatedGraphModel model)
    : AwaitValueChangedMessage<ActivatedGraphModel>(model);
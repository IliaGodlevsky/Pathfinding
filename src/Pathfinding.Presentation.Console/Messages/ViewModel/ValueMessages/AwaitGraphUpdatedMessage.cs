using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal sealed class AwaitGraphUpdatedMessage(GraphInformationModel model)
    : AwaitValueChangedMessage<GraphInformationModel>(model);
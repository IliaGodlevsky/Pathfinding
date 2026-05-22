using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal class AwaitGraphStructureUpdatedMessage(GraphInformationModel model)
    : AwaitValueChangedMessage<GraphInformationModel>(model);
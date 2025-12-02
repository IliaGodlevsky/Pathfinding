using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal class AwaitGraphStructureUpdatedMessage(GraphInformationModel model)
    : AwaitValueChangedMessage<GraphInformationModel>(model);
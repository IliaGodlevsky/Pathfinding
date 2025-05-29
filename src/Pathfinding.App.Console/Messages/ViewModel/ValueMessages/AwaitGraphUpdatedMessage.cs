using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class AwaitGraphUpdatedMessage(GraphInformationModel model) 
    : AwaitValueChangedMessage<GraphInformationModel>(model);
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class AsyncGraphUpdatedMessage(GraphInformationModel model) 
    : AsyncValueChangedMessage<GraphInformationModel, bool>(model);
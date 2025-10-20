using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphUpdatedMessage(GraphInformationModel model)
    : ValueChangedMessage<GraphInformationModel>(model);
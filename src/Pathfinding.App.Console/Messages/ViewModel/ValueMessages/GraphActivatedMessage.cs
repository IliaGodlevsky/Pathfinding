using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.App.Console.Models;
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphActivatedMessage(GraphModel<GraphVertexModel> models) 
    : ValueChangedMessage<GraphModel<GraphVertexModel>>(models);
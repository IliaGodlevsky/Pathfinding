using Pathfinding.App.Console.Models;
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class AsyncGraphActivatedMessage(GraphModel<GraphVertexModel> models) 
    : AsyncValueChangedMessage<GraphModel<GraphVertexModel>>(models);
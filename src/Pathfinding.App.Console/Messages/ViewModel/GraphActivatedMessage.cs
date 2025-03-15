using Pathfinding.App.Console.Models;
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Messages.ViewModel;

internal sealed record class GraphActivatedMessage(GraphModel<GraphVertexModel> Graph);

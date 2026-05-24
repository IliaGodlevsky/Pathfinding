using Pathfinding.Domain.Interface;
using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Messages.View;

internal sealed record GraphActivatedMessage(IGraph<GraphVertexModel> Graph);

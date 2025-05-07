using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Data.Pathfinding;

namespace Pathfinding.App.Console.Models;

internal record ActivatedGraphModel(
    Graph<GraphVertexModel> Graph,
    Neighborhoods Neighborhood,
    SmoothLevels SmoothLevel,
    GraphStatuses Status,
    int GraphId);
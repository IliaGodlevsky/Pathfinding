using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;

namespace Pathfinding.Service.Interface.Requests.Create;

public class CreateGraphRequest<T> where T : IVertex
{
    public string Name { get; set; }

    public Neighborhoods Neighborhood { get; set; }

    public SmoothLevels SmoothLevel { get; set; }

    public GraphStatuses Status { get; set; }

    public IGraph<T> Graph { get; set; }
}
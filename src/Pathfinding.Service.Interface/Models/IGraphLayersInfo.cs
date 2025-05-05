using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.Service.Interface.Models;

public interface IGraphLayersInfo
{
    Neighborhoods Neighborhood { get; }

    SmoothLevels SmoothLevel { get; }
}
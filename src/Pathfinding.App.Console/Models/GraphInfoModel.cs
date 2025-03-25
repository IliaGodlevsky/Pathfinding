using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface.Models;
using ReactiveUI;

namespace Pathfinding.App.Console.Models;

internal sealed class GraphInfoModel : ReactiveObject, IGraphLayersInfo
{
    public int Id { get; set; }

    private string name;
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    private Neighborhoods neighorhood;
    public Neighborhoods Neighborhood
    {
        get => neighorhood;
        set => this.RaiseAndSetIfChanged(ref neighorhood, value);
    }

    private SmoothLevels smoothLevel;
    public SmoothLevels SmoothLevel
    {
        get => smoothLevel;
        set => this.RaiseAndSetIfChanged(ref smoothLevel, value);
    }

    private int width;
    public int Width
    {
        get => width;
        set => this.RaiseAndSetIfChanged(ref width, value);
    }

    private int length;
    public int Length
    {
        get => length;
        set => this.RaiseAndSetIfChanged(ref length, value);
    }

    private int obstacles;
    public int ObstaclesCount
    {
        get => obstacles;
        set => this.RaiseAndSetIfChanged(ref obstacles, value);
    }

    private GraphStatuses status;
    public GraphStatuses Status
    {
        get => status;
        set => this.RaiseAndSetIfChanged(ref status, value);
    }

    public object[] GetProperties()
    {
        return [ Id, Name, Width, Length, Neighborhood,
            SmoothLevel, ObstaclesCount, Status ];
    }
}

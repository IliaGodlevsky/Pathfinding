﻿using Pathfinding.Domain.Core.Enums;
using ReactiveUI;

namespace Pathfinding.App.Console.Models;

internal sealed class GraphInfoModel : ReactiveObject
{
    public int Id { get; set; }

    private string name;
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    private Neighborhoods neighborhood;
    public Neighborhoods Neighborhood
    {
        get => neighborhood;
        set => this.RaiseAndSetIfChanged(ref neighborhood, value);
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

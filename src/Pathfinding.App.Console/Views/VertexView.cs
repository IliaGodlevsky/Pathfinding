using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;
using static Pathfinding.App.Console.Settings;
// ReSharper disable StaticMemberInGenericType

namespace Pathfinding.App.Console.Views;

internal abstract class VertexView<T> : Label
    where T : IVertex
{
    private static readonly Color Background = Enum.Parse<Color>(Default.BackgroundColor);

    protected static readonly ColorScheme ObstacleColor = Create(Default.BackgroundColor);
    protected static readonly ColorScheme RegularColor = Create(Default.RegularVertexColor);
    protected static readonly ColorScheme VisitedColor = Create(Default.VisitedVertexColor);
    protected static readonly ColorScheme EnqueuedColor = Create(Default.EnqueuedVertexColor);
    protected static readonly ColorScheme SourceColor = Create(Default.SourceVertexColor);
    protected static readonly ColorScheme TargetColor = Create(Default.TargetVertexColor);
    protected static readonly ColorScheme TransitColor = Create(Default.TranstiVertexColor);
    protected static readonly ColorScheme PathColor = Create(Default.PathVertexColor);
    protected static readonly ColorScheme CrossedPathColor = Create(Default.CrossedPathColor);

    protected readonly T model;
    protected readonly CompositeDisposable disposables = [];

    protected VertexView(T model)
    {
        model.WhenAnyValue(x => x.Cost)
            .Select(x => x.CurrentCost.ToString())
            .Do(x => Text = x)
            .Subscribe()
            .DisposeWith(disposables);

        this.model = model; 
        var labelWidth = GraphFieldView.DistanceBetweenVertices;
        X = model.Position.GetX() * labelWidth;
        Y = model.Position.GetY();
        Width = labelWidth;
    }

    private static ColorScheme Create(string foreground)
    {
        var foregroundColor = Enum.Parse<Color>(foreground);
        var attribute = Application.Driver.MakeAttribute(foregroundColor, Background);
        return new() { Normal = attribute };
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}

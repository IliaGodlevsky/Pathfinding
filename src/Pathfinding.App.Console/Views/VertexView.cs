using Pathfinding.Domain.Interface;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;
using static Pathfinding.App.Console.Settings;
// ReSharper disable StaticMemberInGenericType

namespace Pathfinding.App.Console.Views;

internal class VertexView<T> : Label
    where T : IVertex
{
    public static readonly Color Background = Enum.Parse<Color>(Default.BackgroundColor);
    public static readonly ColorScheme ObstacleColor = Create(Default.BackgroundColor);
    public static readonly ColorScheme RegularColor = Create(Default.RegularVertexColor);
    public static readonly ColorScheme VisitedColor = Create(Default.VisitedVertexColor);
    public static readonly ColorScheme EnqueuedColor = Create(Default.EnqueuedVertexColor);
    public static readonly ColorScheme SourceColor = Create(Default.SourceVertexColor);
    public static readonly ColorScheme TargetColor = Create(Default.TargetVertexColor);
    public static readonly ColorScheme TransitColor = Create(Default.TranstiVertexColor);
    public static readonly ColorScheme PathColor = Create(Default.PathVertexColor);
    public static readonly ColorScheme CrossedPathColor = Create(Default.CrossedPathColor);

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
        X = model.Position.ElementAtOrDefault(0) * labelWidth;
        Y = model.Position.ElementAtOrDefault(1);
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

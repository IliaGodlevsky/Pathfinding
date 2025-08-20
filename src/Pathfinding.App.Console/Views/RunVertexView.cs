using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class RunVertexView : VertexView<RunVertexModel>
{
    public RunVertexView(RunVertexModel model) : base(model)
    {
        BindTo(x => x.IsObstacle, ObstacleColor, RegularColor, 0);
        BindTo(x => x.IsTarget, TargetColor, RegularColor);
        BindTo(x => x.IsSource, SourceColor, RegularColor);
        BindTo(x => x.IsTransit, TransitColor, RegularColor);
        BindTo(x => x.IsPath, PathColor, VisitedColor);
        BindTo(x => x.IsVisited, VisitedColor, EnqueuedColor);
        BindTo(x => x.IsEnqueued, EnqueuedColor, RegularColor);
        BindTo(x => x.IsCrossedPath, CrossedPathColor, PathColor);
    }

    private void BindTo(Expression<Func<RunVertexModel, bool>> expression,
        ColorScheme toColor, ColorScheme reverseColor, int toSkip = 1)
    {
        model.WhenAnyValue(expression)
           .Skip(toSkip)
           .Select(x => x ? toColor : reverseColor)
           .BindTo(this, x => x.ColorScheme)
           .DisposeWith(disposables);
    }
}

using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphVertexView : VertexView<GraphVertexModel>
{
    public GraphVertexView(GraphVertexModel model)
        : base(model)
    {
        model.WhenAnyValue(
            x => x.IsObstacle,
            x => x.IsTransit,
            x => x.IsTarget,
            x => x.IsSource,
            (isObstacle, isTransit, isTarget, isSource) =>
            {
                if (isObstacle) return ObstacleColor;
                if (isTransit) return TransitColor;
                if (isTarget) return TargetColor;
                if (isSource) return SourceColor;
                return RegularColor;
            })
            .BindTo(this, x => x.ColorScheme)
            .DisposeWith(disposables);
    }
}

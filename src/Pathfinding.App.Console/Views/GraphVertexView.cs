using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphVertexView : VertexView<GraphVertexModel>
{
    private const string ObstacleWall = "███";
    private const string MazeWall = "▓▓▓";
    private const string MazePath = " · ";

    public GraphVertexView(GraphVertexModel model)
        : base(model)
    {
        model.WhenAnyValue(
                x => x.IsObstacle,
                x => x.IsMaze,
                x => x.Neighbors,
                x => x.Cost,
                (isObstacle, isMaze, neighbours, cost) =>
                {
                    if (isObstacle) return ObstacleWall;
                    if (isMaze) return neighbours.Count == 0 ? MazeWall : MazePath;
                    return cost.CurrentCost.ToString();
                })
            .BindTo(this, x => x.Text)
            .DisposeWith(disposables);

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

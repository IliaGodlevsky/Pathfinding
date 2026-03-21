using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphVertexView : VertexView<GraphVertexModel>
{
    private const string ObstacleWall = "███";

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
                    if (isMaze) return $" {ToMazeGlyph(model, neighbours)} ";
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

    private static char ToMazeGlyph(GraphVertexModel model, IReadOnlyCollection<GraphVertexModel> neighbours)
    {
        int x = model.Position.ElementAtOrDefault(0);
        int y = model.Position.ElementAtOrDefault(1);
        bool up = neighbours.Any(n => n.Position.ElementAtOrDefault(0) == x && n.Position.ElementAtOrDefault(1) == y - 1);
        bool right = neighbours.Any(n => n.Position.ElementAtOrDefault(0) == x + 1 && n.Position.ElementAtOrDefault(1) == y);
        bool down = neighbours.Any(n => n.Position.ElementAtOrDefault(0) == x && n.Position.ElementAtOrDefault(1) == y + 1);
        bool left = neighbours.Any(n => n.Position.ElementAtOrDefault(0) == x - 1 && n.Position.ElementAtOrDefault(1) == y);
        int mask = (up ? 1 : 0) | (right ? 2 : 0) | (down ? 4 : 0) | (left ? 8 : 0);
        return mask switch
        {
            0 => '•',
            1 => '╵',
            2 => '╶',
            3 => '└',
            4 => '╷',
            5 => '│',
            6 => '┌',
            7 => '├',
            8 => '╴',
            9 => '┘',
            10 => '─',
            11 => '┴',
            12 => '┐',
            13 => '┤',
            14 => '┬',
            _ => '┼'
        };
    }
}

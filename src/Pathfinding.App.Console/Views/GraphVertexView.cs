using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed class GraphVertexView : VertexView<GraphVertexModel>
    {
        public GraphVertexView(GraphVertexModel model)
            : base(model)
        {
            BindTo(x => x.IsTarget, TargetColor);
            BindTo(x => x.IsSource, SourceColor);
            BindTo(x => x.IsTransit, TransitColor);
            BindTo(x => x.IsObstacle, ObstacleColor);
        }

        private void BindTo(Expression<Func<GraphVertexModel, bool>> expression,
            ColorScheme toColor)
        {
            model.WhenAnyValue(expression)
               .Select(x => x ? toColor : RegularColor)
               .BindTo(this, x => x.ColorScheme)
               .DisposeWith(disposables);
        }
    }
}

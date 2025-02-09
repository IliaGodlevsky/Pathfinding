using Pathfinding.App.Console.Model;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
{
    internal sealed class GraphVertexView : VertexView<GraphVertexModel>
    {
        private readonly CompositeDisposable disposables = [];

        public GraphVertexView(GraphVertexModel model)
            : base(model)
        {
            BindTo(x => x.IsTarget, TargetColor);
            BindTo(x => x.IsSource, SourceColor);
            BindTo(x => x.IsTransit, TransitColor);
            BindTo(x => x.IsObstacle, ObstacleColor);

            model.WhenAnyValue(x => x.Cost)
                .Select(x => x.CurrentCost.ToString())
                .Do(x => Text = x)
                .Subscribe()
                .DisposeWith(disposables);
        }

        private void BindTo(Expression<Func<GraphVertexModel, bool>> expression,
            ColorScheme toColor)
        {
            model.WhenAnyValue(expression)
               .Select(x => x ? toColor : RegularColor)
               .BindTo(this, x => x.ColorScheme)
               .DisposeWith(disposables);
        }

        protected override void Dispose(bool disposing)
        {
            disposables.Dispose();
            base.Dispose(disposing);
        }
    }
}

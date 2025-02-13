using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;
using static Pathfinding.App.Console.Settings;

namespace Pathfinding.App.Console.View
{
    internal abstract class VertexView<T> : Label
        where T : IVertex
    {
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
        protected const int LabelWidth = GraphFieldView.DistanceBetweenVertices;
        protected readonly CompositeDisposable disposables = [];

        protected VertexView(T model)
        {
            model.WhenAnyValue(x => x.Cost)
                .Select(x => x.CurrentCost.ToString())
                .Do(x => Text = x)
                .Subscribe()
                .DisposeWith(disposables);

            this.model = model;
            X = model.Position.GetX() * LabelWidth;
            Y = model.Position.GetY();
            Width = LabelWidth;
        }

        private static ColorScheme Create(string foreground)
        {
            var driver = Application.Driver;
            var backgroundColor = Enum.Parse<Color>(Settings.Default.BackgroundColor);
            var attribute = driver.MakeAttribute(Enum.Parse<Color>(foreground), backgroundColor);
            return new() { Normal = attribute };
        }

        protected override void Dispose(bool disposing)
        {
            disposables.Dispose();
            base.Dispose(disposing);
        }
    }
}

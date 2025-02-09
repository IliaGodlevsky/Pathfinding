using Pathfinding.App.Console.ViewModel.Interface;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.View;

internal sealed partial class GraphParametresView : FrameView
{
    private static readonly InclusiveValueRange<int> WidthRange = (51, 1);
    private static readonly InclusiveValueRange<int> LengthRange = (48, 1);
    private static readonly InclusiveValueRange<int> ObstaclesRange = (99, 0);

    private readonly IRequireGraphParametresViewModel viewModel;
    private readonly CompositeDisposable disposables = [];

    public GraphParametresView(IRequireGraphParametresViewModel viewModel)
    {
        this.viewModel = viewModel;
        Initialize();
        BindTo(obstaclesInput, x => x.Obstacles, ObstaclesRange);
        BindTo(graphWidthInput, x => x.Width, WidthRange);
        BindTo(graphLengthInput, x => x.Length, LengthRange);
        this.Events().VisibleChanged
            .Where(x => Visible)
            .Do(x =>
            {
                graphWidthInput.Text = string.Empty;
                graphLengthInput.Text = string.Empty;
                obstaclesInput.Text = string.Empty;
            })
            .Subscribe();
    }

    private void BindTo(TextField field,
        Expression<Func<IRequireGraphParametresViewModel, int>> expression,
        InclusiveValueRange<int> range)
    {
        field.Events()
            .TextChanging
            .Select(x =>
            {
                if (int.TryParse(x.NewText.ToString(), out var value))
                {
                    var returned = range.ReturnInRange(value);
                    x.NewText = returned.ToString();
                    return returned;
                }
                return -1;
            })
            .BindTo(viewModel, expression)
            .DisposeWith(disposables);
    }
}

using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Shared.Primitives;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphParametresView : FrameView
{
    private readonly IRequireGraphParametresViewModel viewModel;
    private readonly CompositeDisposable disposables = [];

    public GraphParametresView(IRequireGraphParametresViewModel viewModel)
    {
        this.viewModel = viewModel;
        Initialize();
        BindTo(obstaclesInput, x => x.Obstacles);
        BindTo(graphWidthInput, x => x.Width);
        BindTo(graphLengthInput, x => x.Length);
        BindTo(upperCostInput, x => x.Range, true);
        BindTo(lowerCostInput, x => x.Range);
    }

    private void BindTo(TextField field, Expression<Func<IRequireGraphParametresViewModel, int>> expression)
    {
        var compiled = expression.Compile();
        var propertyName = ((MemberExpression)expression.Body).Member.Name;
        field.Events().TextChanging
            .Select(x => int.TryParse(x.NewText.ToString(), out var value) ? value : default)
            .BindTo(viewModel, expression)
            .DisposeWith(disposables);
        viewModel.Events().PropertyChanged
            .Where(x => x.PropertyName == propertyName)
            .Do(x =>
            {
                Application.MainLoop.Invoke(() =>
                {
                    var propertyValue = compiled(viewModel).ToString();
                    if (field.Text != propertyValue)
                    {
                        field.Text = propertyValue;
                    }
                });
            })
            .Subscribe()
            .DisposeWith(disposables);
    }

    private void BindTo(TextField field, Expression<Func<IRequireGraphParametresViewModel, InclusiveValueRange<int>>> expression, bool isUpper = false)
    {
        var compiled = expression.Compile();
        var propertyName = ((MemberExpression)expression.Body).Member.Name;
        field.Events().TextChanging
            .Select(x =>
            {
                var range = compiled(viewModel);
                if (int.TryParse(x.NewText.ToString(), out var value))
                {
                    return isUpper
                        ? new InclusiveValueRange<int>(value, range.LowerValueOfRange)
                        : new InclusiveValueRange<int>(range.UpperValueOfRange, value);
                }
                return range;
            })
            .BindTo(viewModel, expression)
            .DisposeWith(disposables);
        viewModel.Events().PropertyChanged
            .Where(x => x.PropertyName == propertyName)
            .Do(x =>
            {
                Application.MainLoop.Invoke(() =>
                {
                    var property = compiled(viewModel);
                    var value = (isUpper ? property.UpperValueOfRange : property.LowerValueOfRange).ToString();
                    if (field.Text != value)
                    {
                        field.Text = value;
                    }
                });
            })
            .Subscribe()
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        graphWidthInput.KeyPress -= KeyRestriction;
        graphLengthInput.KeyPress -= KeyRestriction;
        obstaclesInput.KeyPress -= KeyRestriction;
        upperCostInput.KeyPress -= KeyRestriction;
        lowerCostInput.KeyPress -= KeyRestriction;
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
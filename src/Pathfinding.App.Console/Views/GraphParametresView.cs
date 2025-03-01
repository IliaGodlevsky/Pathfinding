using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Disposables;
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
                    var propertyValue = compiled(viewModel);
                    bool parsed = int.TryParse(field.Text.ToString(), out var value);
                    if (parsed && value != propertyValue)
                    {
                        field.Text = propertyValue.ToString();
                    }
                });
            })
            .Subscribe()
            .DisposeWith(disposables);
    }
}
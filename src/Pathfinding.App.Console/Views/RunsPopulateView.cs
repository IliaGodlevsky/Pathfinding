using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Globalization;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunsPopulateView : FrameView
{
    private const double DefaultWeight = 1;

    private readonly IRequirePopulationViewModel populateViewModel;
    private readonly CompositeDisposable disposables = [];

    public RunsPopulateView(
        [KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRequirePopulationViewModel populateViewModel)
    {
        Initialize();
        this.populateViewModel = populateViewModel;
        weightLabel.DisposeWith(disposables);
        weightTextField.DisposeWith(disposables);
        toWeightTextField.DisposeWith(disposables);
        toWeightLabel.DisposeWith(disposables);
        stepLabel.DisposeWith(disposables);
        stepTextField.DisposeWith(disposables);

        BindTo(weightTextField, x => x.FromWeight);
        BindTo(toWeightTextField, x => x.ToWeight);
        BindTo(stepTextField, x => x.Step);

        messenger.RegisterHandler<OpenRunsPopulateViewMessage>(this, OnRunPopulateOpen).DisposeWith(disposables);
        messenger.RegisterHandler<CloseRunPopulateViewMessage>(this, OnRunPopulateViewClosed).DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void BindTo(TextField field,
        Expression<Func<IRequirePopulationViewModel, double?>> expression)
    {
        var propertyName = ((MemberExpression)expression.Body).Member.Name;
        field.Events().TextChanging
            .DistinctUntilChanged()
            .Select(x => double.TryParse(x.NewText.ToString(), out var value) ? value : default(double?))
            .BindTo(populateViewModel, expression)
            .DisposeWith(disposables);
        var compiled = expression.Compile();
        populateViewModel.Events().PropertyChanged
            .Where(x => x.PropertyName == propertyName)
            .Do(_ =>
            {
                Application.MainLoop.Invoke(() =>
                {
                    var propertyValue = compiled(populateViewModel);
                    var parsed = double.TryParse(field.Text.ToString(), out var value);
                    if (parsed && value != propertyValue)
                    {
                        field.Text = propertyValue.ToString();
                    }
                });
            })
            .Subscribe()
            .DisposeWith(disposables);
    }

    private void OnRunPopulateOpen(OpenRunsPopulateViewMessage msg)
    {
        SetDefaults();
        Visible = true;
    }

    private void OnRunPopulateViewClosed(CloseRunPopulateViewMessage msg) => Close();

    private void SetDefaults()
    {
        weightTextField.Text = DefaultWeight.ToString(CultureInfo.InvariantCulture);
        toWeightTextField.Text = DefaultWeight.ToString(CultureInfo.InvariantCulture);
        stepTextField.Text = "0";
    }

    private void Close()
    {
        populateViewModel.FromWeight = null;
        populateViewModel.ToWeight = null;
        populateViewModel.Step = null;
        Visible = false;
    }
}

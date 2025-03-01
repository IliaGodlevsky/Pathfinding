using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunsPopulateView : FrameView
    {
        private const double DefaultWeight = 1;

        private readonly CompositeDisposable disposables = [];
        private readonly IRequireHeuristicsViewModel heuristicsViewModel;

        public RunsPopulateView(
            [KeyFilter(KeyFilters.Views)] IMessenger messenger,
            IRequireHeuristicsViewModel heuristicsViewModel)
        {
            Initialize();
            this.heuristicsViewModel = heuristicsViewModel;

            BindTo(weightTextField, x => x.FromWeight);
            BindTo(toWeightTextField, x => x.ToWeight);
            BindTo(stepTextField, x => x.Step);

            messenger.Register<OpenRunsPopulateViewMessage>(this, OnRunPopulateOpen);
            messenger.Register<CloseRunPopulateViewMessage>(this, OnRunPopulateViewClosed);
            messenger.Register<CloseRunCreateViewMessage>(this, OnRunCreateViewClosed);
            messenger.Register<OpenHeuristicsViewMessage>(this, OnHeuristicsViewOpen);
        }

        private void BindTo(TextField field, 
            Expression<Func<IRequireHeuristicsViewModel, double?>> expression)
        {
            var compiled = expression.Compile();
            var propertyName = ((MemberExpression)expression.Body).Member.Name;
            field.Events().TextChanging
                .DistinctUntilChanged()
                .Select(x => double.TryParse(x.NewText.ToString(), out var value) ? value : default(double?))
                .BindTo(heuristicsViewModel, expression)
                .DisposeWith(disposables);
            heuristicsViewModel.Events().PropertyChanged
                .Where(x => x.PropertyName == propertyName)
                .Do(x =>
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        var propertyValue = compiled(heuristicsViewModel);
                        bool parsed = double.TryParse(field.Text.ToString(), out var value);
                        if (parsed && value != propertyValue)
                        {
                            field.Text = propertyValue.ToString();
                        }
                    });
                })
                .Subscribe()
                .DisposeWith(disposables);
        }

        private void OnHeuristicsViewOpen(object recipient, OpenHeuristicsViewMessage msg) => SetDefaults();

        private void OnRunPopulateOpen(object recipient, OpenRunsPopulateViewMessage msg)
        {
            SetDefaults();
            Visible = true;
        }

        private void OnRunPopulateViewClosed(object recipient, CloseRunPopulateViewMessage msg) => Close();

        private void OnRunCreateViewClosed(object recipient, CloseRunCreateViewMessage msg) => Close();

        private void SetDefaults()
        {
            weightTextField.Text = DefaultWeight.ToString();
            toWeightTextField.Text = DefaultWeight.ToString();
            stepTextField.Text = "0";
            heuristicsViewModel.FromWeight = DefaultWeight;
            heuristicsViewModel.ToWeight = DefaultWeight;
            heuristicsViewModel.Step = 0;
        }

        private void Close()
        {
            heuristicsViewModel.FromWeight = null;
            heuristicsViewModel.ToWeight = null;
            heuristicsViewModel.Step = null;
            Visible = false;
        }
    }
}

using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunsPopulateView : FrameView
    {
        private const double DefaultWeight = 1;
        private static readonly InclusiveValueRange<double> WeightRange = (5, 0);

        private readonly CompositeDisposable disposables = [];
        private readonly IRequireHeuristicsViewModel heuristicsViewModel;

        public RunsPopulateView(
            [KeyFilter(KeyFilters.Views)] IMessenger messenger,
            IRequireHeuristicsViewModel heuristicsViewModel)
        {
            Initialize();
            this.heuristicsViewModel = heuristicsViewModel;

            weightTextField.Events().TextChanging
                .Select(e => GetValidValue(e, ClampWeight))
                .BindTo(heuristicsViewModel, x => x.FromWeight)
                .DisposeWith(disposables);

            toWeightTextField.Events().TextChanging
                .Select(e => GetValidValue(e, ClampToWeight))
                .BindTo(heuristicsViewModel, x => x.ToWeight)
                .DisposeWith(disposables);

            stepTextField.Events().TextChanging
                .Select(e => GetValidValue(e, ClampStep))
                .BindTo(heuristicsViewModel, x => x.Step)
                .DisposeWith(disposables);

            Observable.Merge(
                weightTextField.Events().TextChanged,
                toWeightTextField.Events().TextChanged)
                .Do(_ => UpdateStepAndSync())
                .Subscribe()
                .DisposeWith(disposables);

            messenger.Register<OpenRunsPopulateViewMessage>(this, OnOpen);
            messenger.Register<CloseRunPopulateViewMessage>(this, OnViewClosed);
            messenger.Register<CloseRunCreateViewMessage>(this, OnRunCreationViewClosed);
            messenger.Register<OpenHeuristicsViewMessage>(this, OnHeuristicsOpen);
        }

        private static double GetValidValue(TextChangingEventArgs e, Func<double, double> clampFunc)
        {
            var text = e.NewText.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return -1;

            if (!double.TryParse(text, out var value))
                return -1;

            var clamped = clampFunc(value);
            if (clamped != value)
            {
                value = clamped;
                e.NewText = value.ToString();
            }
            return value;
        }

        private double ClampWeight(double value) =>
            WeightRange.Contains(value) ? value : WeightRange.ReturnInRange(value);

        private double ClampToWeight(double value)
        {
            value = ClampWeight(value);
            if (double.TryParse(weightTextField.Text.ToString(), out var weight) && value < weight)
                value = weight;
            return value;
        }

        private double ClampStep(double value)
        {
            if (double.TryParse(weightTextField.Text.ToString(), out var weight) &&
                double.TryParse(toWeightTextField.Text.ToString(), out var toWeight))
            {
                var maxStep = Math.Round(toWeight - weight, 3);
                return value > maxStep ? maxStep : value;
            }
            return value;
        }

        private void UpdateStepAndSync()
        {
            if (!double.TryParse(weightTextField.Text.ToString(), out var weight) ||
                !double.TryParse(toWeightTextField.Text.ToString(), out var toWeight))
            {
                return;
            }

            if (weight >= toWeight)
            {
                toWeightTextField.Text = weight.ToString();
                stepTextField.Text = "0";
                return;
            }

            var step = double.TryParse(stepTextField.Text.ToString(), out var s) ? s : 0;
            var calculatedStep = Math.Round(toWeight - weight, 3);
            if (step == 0 || step > calculatedStep)
                stepTextField.Text = calculatedStep.ToString();
        }

        private void OnHeuristicsOpen(object recipient, OpenHeuristicsViewMessage msg)
        {
            SetDefaults();
        }

        private void OnOpen(object recipient, OpenRunsPopulateViewMessage msg)
        {
            SetDefaults();
            Visible = true;
        }

        private void OnViewClosed(object recipient, CloseRunPopulateViewMessage msg) => Close();

        private void OnRunCreationViewClosed(object recipient, CloseRunCreateViewMessage msg) => Close();

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

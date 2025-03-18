using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunsPopulateView : FrameView
    {
        private const double DefaultWeight = 1;

        private readonly IRequirePopulationViewModel populateViewModel;

        public RunsPopulateView(
            [KeyFilter(KeyFilters.Views)] IMessenger messenger,
            IRequirePopulationViewModel populateViewModel)
        {
            Initialize();
            this.populateViewModel = populateViewModel;

            BindTo(weightTextField, x => x.FromWeight);
            BindTo(toWeightTextField, x => x.ToWeight);
            BindTo(stepTextField, x => x.Step);

            messenger.Register<OpenRunsPopulateViewMessage>(this, OnRunPopulateOpen);
            messenger.Register<CloseRunPopulateViewMessage>(this, OnRunPopulateViewClosed);
            messenger.Register<CloseRunCreateViewMessage>(this, OnRunCreateViewClosed);
        }

        private void BindTo(TextField field,
            Expression<Func<IRequirePopulationViewModel, double?>> expression)
        {
            var compiled = expression.Compile();
            var propertyName = ((MemberExpression)expression.Body).Member.Name;
            field.Events().TextChanging
                .DistinctUntilChanged()
                .Select(x => double.TryParse(x.NewText.ToString(), out var value) ? value : default(double?))
                .BindTo(populateViewModel, expression);
            populateViewModel.Events().PropertyChanged
                .Where(x => x.PropertyName == propertyName)
                .Do(x =>
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        var propertyValue = compiled(populateViewModel);
                        bool parsed = double.TryParse(field.Text.ToString(), out var value);
                        if (parsed && value != propertyValue)
                        {
                            field.Text = propertyValue.ToString();
                        }
                    });
                })
                .Subscribe();
        }

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
        }

        private void Close()
        {
            populateViewModel.FromWeight = null;
            populateViewModel.ToWeight = null;
            populateViewModel.Step = null;
            Visible = false;
        }
    }
}

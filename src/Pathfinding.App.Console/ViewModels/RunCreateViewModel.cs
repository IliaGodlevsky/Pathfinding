using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;

// ReSharper disable PossibleInvalidOperationException
// ReSharper disable RedundantAssignment
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable UnusedMember.Global

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunCreateViewModel : ViewModel,
    IRunCreateViewModel,
    IRequireHeuristicsViewModel,
    IRequireStepRuleViewModel,
    IRequirePopulationViewModel,
    IDisposable
{
    private sealed record AlgorithmBuildInfo(
        Algorithms Algorithm,
        StepRules? StepRule,
        Heuristics? Heuristics = null,
        double? Weight = null) : IAlgorithmBuildInfo;

    private readonly IStatisticsRequestService statisticsService;
    private readonly IMessenger messenger;
    private readonly IAlgorithmsFactory algorithmsFactory;
    private readonly CompositeDisposable disposables = [];

    public ReactiveCommand<Unit, Unit> CreateRunCommand { get; }

    public ObservableCollection<Algorithms> SelectedAlgorithms { get; } = [];

    IList<Algorithms> IRunCreateViewModel.SelectedAlgorithms => SelectedAlgorithms;

    public IReadOnlyCollection<Heuristics> AllowedHeuristics { get; }

    public IReadOnlyList<Algorithms> AllowedAlgorithms { get; }

    public IReadOnlyCollection<StepRules> AllowedStepRules { get; }

    IList<Heuristics> IRequireHeuristicsViewModel.AppliedHeuristics => AppliedHeuristics;

    public ObservableCollection<Heuristics> AppliedHeuristics { get; } = [];

    private double? fromWeight;
    public double? FromWeight
    {
        get => fromWeight;
        set { SetFromWeight(ref fromWeight, value); this.RaisePropertyChanged(); }
    }

    private double? toWeight;
    public double? ToWeight
    {
        get => toWeight;
        set { SetToWeight(ref toWeight, value); this.RaisePropertyChanged(); }
    }

    private double? step;
    public double? Step
    {
        get => step;
        set { SetStep(ref step, value); this.RaisePropertyChanged(); }
    }

    private StepRules? stepRule;

    public StepRules? StepRule
    {
        get => stepRule;
        set => this.RaiseAndSetIfChanged(ref stepRule, value);
    }

    private ActiveGraph activatedGraph = ActiveGraph.Empty;
    private ActiveGraph ActivatedGraph
    {
        get => activatedGraph;
        set => this.RaiseAndSetIfChanged(ref activatedGraph, value);
    }

    public IReadOnlyDictionary<Algorithms, AlgorithmRequirements> Requirements { get; }

    public RunCreateViewModel(
        IStatisticsRequestService statisticsService,
        IAlgorithmsFactory algorithmsFactory,
        IHeuristicsFactory heuristicsFactory,
        IStepRuleFactory stepRuleFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger) : base(logger)
    {
        this.messenger = messenger;
        this.statisticsService = statisticsService;
        this.algorithmsFactory = algorithmsFactory;
        AllowedHeuristics = heuristicsFactory.Allowed;
        AllowedAlgorithms = algorithmsFactory.Allowed;
        AllowedStepRules = stepRuleFactory.Allowed;
        Requirements = algorithmsFactory.Requirements;

        CreateRunCommand = ReactiveCommand.CreateFromTask(CreateAlgorithm, CanCreateAlgorithm());
        
        messenger.RegisterHandler<GraphActivatedMessage>(this, OnGraphActivated).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
    }

    private IObservable<bool> CanCreateAlgorithm()
    {
        return this.WhenAnyValue(
            x => x.ActivatedGraph, x => x.AppliedHeuristics.Count,
            x => x.FromWeight, x => x.ToWeight,
            x => x.Step, x => x.SelectedAlgorithms.Count,
            (graph, heuristicsCount, weight, to, s, algorithmsCount) =>
            {
                var canExecute = graph != ActiveGraph.Empty && algorithmsCount > 0;
                var requiresHeuristics = SelectedAlgorithms.All(x =>
                    Requirements[x] == AlgorithmRequirements.RequiresAll
                    || Requirements[x] == AlgorithmRequirements.RequiresHeuristics);
                if (canExecute && requiresHeuristics)
                {
                    canExecute = canExecute && heuristicsCount > 0;
                    if (canExecute && s != null && weight != null && to != null)
                    {
                        canExecute = canExecute
                            && weight > 0
                            && to > 0
                            && s >= 0;
                        if (to - weight > 0 && s == 0)
                        {
                            canExecute = false;
                        }
                    }
                }
                return canExecute;
            }
        );
    }

    private void OnGraphActivated(GraphActivatedMessage msg)
    {
        ActivatedGraph = msg.Value.ActiveGraph;
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraph.Id))
        {
            ActivatedGraph = ActiveGraph.Empty;
        }
    }

    private void SetFromWeight(ref double? field, double? value)
    {
        if (value == null)
        {
            field = null;
            return;
        }
        var weightRange = GetWeightRange();
        field = weightRange.ReturnInRange(value.Value);
        if (toWeight < field)
        {
            ToWeight = field;
        }
        if (Step == 0 && ToWeight != field
            || ToWeight - field < Step)
        {
            Step = ToWeight - field;
        }
    }

    private void SetToWeight(ref double? field, double? value)
    {
        if (value == null)
        {
            field = null;
            return;
        }
        field = value > fromWeight ? value : fromWeight;
        if (field != null)
        {
            var weightRange = GetWeightRange();
            field = weightRange.ReturnInRange(field.Value);
        }
        var amplitude = field - fromWeight;
        if (field == fromWeight && Step != amplitude
            || amplitude < Step
            || (Step == 0 && amplitude > 0))
        {
            Step = amplitude;
        }
    }

    private void SetStep(ref double? field, double? value)
    {
        var amplitude = toWeight - fromWeight;
        field = amplitude < value ? amplitude : value;
    }

    private AlgorithmBuildInfo[] GetBuildInfo(double? weight)
    {
        if (AppliedHeuristics.Count == 0)
        {
            return [.. SelectedAlgorithms.Select(algo => new AlgorithmBuildInfo(algo, StepRule))];
        }
        return [.. AppliedHeuristics
            .SelectMany(heuristic => SelectedAlgorithms
                .Select(algo => new AlgorithmBuildInfo(algo, StepRule, heuristic, weight)))];
    }

    private async Task CreateAlgorithm()
    {
        var range = RequestRange();
        if (range.Length < 2)
        {
            log.Info(Resource.RangeIsNotSetMsg);
            return;
        }

        var statistics = EnumerateWeights()
            .SelectMany(GetBuildInfo)
            .Select(buildInfo => CreateStatistics(range, buildInfo))
            .ToArray();

        await ExecuteSafe(async () =>
        {
            using var cts = new CancellationTokenSource(GetTimeout(statistics.Length));
            var result = await statisticsService.CreateStatisticsAsync(statistics, cts.Token)
                .ConfigureAwait(false);
            messenger.Send(new RunsCreatedMessaged([.. result]));
        }).ConfigureAwait(false);
    }

    private GraphVertexModel[] RequestRange()
    {
        var rangeMessage = new PathfindingRangeRequestMessage();
        messenger.Send(rangeMessage);
        return rangeMessage.Response;
    }

    private IEnumerable<double?> EnumerateWeights()
    {
        double from = FromWeight ?? 0;
        double to = ToWeight ?? 0;
        double weightStep = Step ?? 1;
        int limit = Step == 0 ? 0 : (int)Math.Ceiling((to - from) / weightStep);

        for (int i = 0; i <= limit; i++)
        {
            double val = from + weightStep * i;
            yield return val == 0 ? null : Math.Round(val, 2);
        }
    }

    private CreateStatisticsRequest CreateStatistics(
        GraphVertexModel[] range,
        AlgorithmBuildInfo buildInfo)
    {
        int visitedCount = 0;
        void OnVertexProcessed(EventArgs e) => visitedCount++;

        var factory = algorithmsFactory.GetAlgorithmFactory(buildInfo.Algorithm);
        var algo = factory.CreateAlgorithm(range, buildInfo);
        algo.VertexProcessed += OnVertexProcessed;
        var status = RunStatuses.Success;
        var path = NullGraphPath.Interface;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            path = algo.FindPath();
        }
        catch (PathfindingException ex)
        {
            status = RunStatuses.Failure;
            log.Warn(ex);
        }
        catch (Exception ex)
        {
            status = RunStatuses.Failure;
            log.Error(ex);
        }
        finally
        {
            stopwatch.Stop();
            algo.VertexProcessed -= OnVertexProcessed;
        }

        return new()
        {
            Algorithm = buildInfo.Algorithm,
            Cost = path.Cost,
            Steps = path.Count,
            StepRule = buildInfo.StepRule,
            Heuristics = buildInfo.Heuristics,
            Weight = buildInfo.Weight,
            Visited = visitedCount,
            Elapsed = stopwatch.Elapsed,
            ResultStatus = status,
            GraphId = ActivatedGraph.Id
        };
    }

    private static InclusiveValueRange<double> GetWeightRange()
    {
        return (Settings.Default.UpperValueOfHeuristicWeightRange,
            Settings.Default.LowerValueOfHeuristicWeightRange);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}

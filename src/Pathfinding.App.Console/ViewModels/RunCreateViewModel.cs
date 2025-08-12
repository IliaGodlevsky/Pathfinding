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
using Pathfinding.Infrastructure.Data.Pathfinding;
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

internal sealed class RunCreateViewModel : BaseViewModel,
    IRunCreateViewModel,
    IRequireHeuristicsViewModel,
    IRequireStepRuleViewModel,
    IRequirePopulationViewModel,
    IDisposable
{
    private static readonly InclusiveValueRange<double> WeightRange = (5, 0);

    private sealed record AlgorithmBuildInfo(
        Algorithms Algorithm,
        Heuristics? Heuristics,
        double? Weight,
        StepRules? StepRule) : IAlgorithmBuildInfo;

    private readonly IRequestService<GraphVertexModel> service;
    private readonly IMessenger messenger;
    private readonly IAlgorithmsFactory algorithmsFactory;
    private readonly CompositeDisposable disposables = [];

    public ReactiveCommand<Unit, Unit> CreateRunCommand { get; }

    private Algorithms? algorithm;
    public Algorithms? Algorithm
    {
        get => algorithm;
        set => this.RaiseAndSetIfChanged(ref algorithm, value);
    }

    public IReadOnlyCollection<Heuristics> AllowedHeuristics { get; }

    public IReadOnlyCollection<Algorithms> AllowedAlgorithms { get; }

    public IReadOnlyCollection<StepRules> AllowedStepRules { get; }

    public ObservableCollection<Heuristics?> AppliedHeuristics { get; } = [];

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

    private int ActivatedGraphId { get; set; }

    private Graph<GraphVertexModel> graph = Graph<GraphVertexModel>.Empty;

    private Graph<GraphVertexModel> Graph
    {
        get => graph;
        set => this.RaiseAndSetIfChanged(ref graph, value);
    }

    public RunCreateViewModel(IRequestService<GraphVertexModel> service,
        IAlgorithmsFactory algorithmsFactory,
        IHeuristicsFactory heuristicsFactory,
        IStepRuleFactory stepRuleFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger) : base(logger)
    {
        this.messenger = messenger;
        this.service = service;
        this.algorithmsFactory = algorithmsFactory;
        AllowedHeuristics = heuristicsFactory.Allowed;
        AllowedAlgorithms = algorithmsFactory.Allowed;
        AllowedStepRules = stepRuleFactory.Allowed;
        CreateRunCommand = ReactiveCommand.CreateFromTask(CreateAlgorithm, CanCreateAlgorithm());
        messenger.RegisterHandler<GraphActivatedMessage>(this, OnGraphActivated).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
    }

    private IObservable<bool> CanCreateAlgorithm()
    {
        return this.WhenAnyValue(
            x => x.Graph, x => x.AppliedHeuristics.Count,
            x => x.FromWeight, x => x.ToWeight,
            x => x.Step, x => x.Algorithm,
            (g, count, weight, to, s, algo) =>
            {
                var canExecute = g != Graph<GraphVertexModel>.Empty
                    && algo != null
                    && Enum.IsDefined(algo.Value);
                if (count > 0)
                {
                    canExecute = canExecute && count > 1;
                    if (s != null && weight != null && to != null)
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

    private void OnGraphActivated(object recipient, GraphActivatedMessage msg)
    {
        Graph = msg.Value.Graph;
        ActivatedGraphId = msg.Value.GraphId;
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraphId))
        {
            Graph = Graph<GraphVertexModel>.Empty;
            ActivatedGraphId = 0;
        }
    }

    private void SetFromWeight(ref double? field, double? value)
    {
        if (value == null)
        {
            field = null;
            return;
        }
        field = WeightRange.ReturnInRange(value.Value);
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
            field = WeightRange.ReturnInRange(field.Value);
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
        return AppliedHeuristics.Count == 0
            ? [new (Algorithm.Value, null, null, StepRule)]
            : [.. AppliedHeuristics
                .Where(x => x is not null)
                .Select(x => new AlgorithmBuildInfo(Algorithm.Value, x, weight, StepRule))];
    }

    private async Task CreateAlgorithm()
    {
        var rangeMessage = new PathfindingRangeRequestMessage();
        messenger.Send(rangeMessage);
        var range = rangeMessage.Response;

        if (range.Length > 1)
        {
            var visitedCount = 0;
            void OnVertexProcessed(EventArgs e) => visitedCount++;
            var status = RunStatuses.Success;
            var from = FromWeight ?? 0;
            var to = ToWeight ?? 0;
            var weightStep = Step ?? 1;
            var limit = Step == 0 ? 0 : (int)Math.Ceiling((to - from) / weightStep);
            var list = new List<CreateStatisticsRequest>();
            for (var i = 0; i <= limit; i++)
            {
                var val = from + weightStep * i;
                double? weight = val == 0 ? null : Math.Round(val, 2);
                foreach (var buildInfo in GetBuildInfo(weight))
                {
                    visitedCount = 0;
                    var factory = algorithmsFactory.GetAlgorithmFactory(buildInfo.Algorithm);
                    var algo = factory.CreateAlgorithm(range, buildInfo);
                    algo.VertexProcessed += OnVertexProcessed;
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

                    list.Add(new()
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
                        GraphId = ActivatedGraphId
                    });
                }
            }
            await ExecuteSafe(async () =>
            {
                var timeout = Timeout * list.Count;
                using var cts = new CancellationTokenSource(timeout);
                var result = await service.CreateStatisticsAsync(list, cts.Token)
                    .ConfigureAwait(false);
                messenger.Send(new RunsCreatedMessaged([.. result]));
            }).ConfigureAwait(false);
        }
        else
        {
            log.Info(Resource.RangeIsNotSetMsg);
        }
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}

using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Extensions;
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

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunCreateViewModel : BaseViewModel,
    IRunCreateViewModel,
    IRequireHeuristicsViewModel,
    IRequireStepRuleViewModel,
    IRequirePopulationViewModel
{
    private static readonly InclusiveValueRange<double> WeightRange = (5, 0);

    private sealed record class AlgorithmBuildInfo(
        Algorithms Algorithm, 
        Heuristics? Heuristics,
        double? Weight, 
        StepRules? StepRule) : IAlgorithmBuildInfo;

    private readonly IRequestService<GraphVertexModel> service;
    private readonly IMessenger messenger;
    private readonly ILog logger;

    public ReactiveCommand<Unit, Unit> CreateRunCommand { get; }

    private Algorithms? algorithm;
    public Algorithms? Algorithm
    {
        get => algorithm;
        set => this.RaiseAndSetIfChanged(ref algorithm, value);
    }

    public IReadOnlyCollection<Heuristics> AllowedHeuristics { get; }

    public IReadOnlyList<Algorithms> AllowedAlgorithms { get; }

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
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger)
    {
        this.messenger = messenger;
        this.service = service;
        this.logger = logger;
        CreateRunCommand = ReactiveCommand.CreateFromTask(CreateAlgorithm, CanCreateAlgorithm());
        AllowedAlgorithms = [.. Enum.GetValues<Algorithms>().OrderBy(x => x.GetOrder())];
        AllowedHeuristics = Enum.GetValues<Heuristics>();
        messenger.Register<GraphActivatedMessage>(this, OnGraphActivated);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
    }

    private IObservable<bool> CanCreateAlgorithm()
    {
        return this.WhenAnyValue(
            x => x.Graph, x => x.AppliedHeuristics.Count,
            x => x.FromWeight, x => x.ToWeight, 
            x => x.Step, x => x.Algorithm,
            (graph, count, weight, to, step, algorithm) =>
            {
                bool canExecute = graph != Graph<GraphVertexModel>.Empty
                    && algorithm != null
                    && Enum.IsDefined(algorithm.Value);
                if (count > 0)
                {
                    canExecute = canExecute && count > 1;
                    if (step != null && weight != null && to != null)
                    {
                        canExecute = canExecute
                            && weight > 0
                            && to > 0
                            && step >= 0;
                        if (to - weight > 0 && step == 0)
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
        Graph = new Graph<GraphVertexModel>(msg.Graph.Vertices,
            msg.Graph.DimensionSizes);
        ActivatedGraphId = msg.Graph.Id;
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        if (msg.GraphIds.Contains(ActivatedGraphId))
        {
            Graph = Graph<GraphVertexModel>.Empty;
            ActivatedGraphId = 0;
        }
    }

    private void SetFromWeight(ref double? field, double? value)
    {
        if (value == null)
        {
            field = value;
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
            field = value;
            return;
        }
        field = value > fromWeight ? value : fromWeight;
        field = WeightRange.ReturnInRange(field.Value);
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
            ? [new AlgorithmBuildInfo(Algorithm.Value, default, weight, StepRule)]
            : AppliedHeuristics.Where(x => x is not null)
                .Select(x => new AlgorithmBuildInfo(Algorithm.Value, x, weight, StepRule))
                .ToArray();
    }

    private async Task CreateAlgorithm()
    {
        var pathfindingRange = (await service.ReadRangeAsync(ActivatedGraphId)
            .ConfigureAwait(false))
            .Select(x => Graph.Get(x.Position))
            .ToList();

        if (pathfindingRange.Count > 1)
        {
            int visitedCount = 0;
            void OnVertexProcessed(EventArgs e) => visitedCount++;
            var status = RunStatuses.Success;
            double from = FromWeight ?? 0;
            double to = ToWeight ?? 0;
            double step = Step ?? 1;
            int limit = Step == 0 ? 0 : (int)Math.Ceiling((to - from) / step);
            var list = new List<CreateStatisticsRequest>();
            for (int i = 0; i <= limit; i++)
            {
                var val = from + step * i;
                double? weight = val == 0 ? null : Math.Round(val, 2);
                foreach (var buildInfo in GetBuildInfo(weight))
                {
                    visitedCount = 0;
                    var algorithm = buildInfo.ToAlgorithm(pathfindingRange);
                    algorithm.VertexProcessed += OnVertexProcessed;
                    var path = NullGraphPath.Interface;
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        path = algorithm.FindPath();
                    }
                    catch (PathfindingException ex)
                    {
                        status = RunStatuses.Failure;
                        logger.Warn(ex);
                    }
                    catch (Exception ex)
                    {
                        status = RunStatuses.Failure;
                        logger.Error(ex);
                    }
                    finally
                    {
                        stopwatch.Stop();
                        algorithm.VertexProcessed -= OnVertexProcessed;
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
                var result = await service.CreateStatisticsAsync(list)
                    .ConfigureAwait(false);
                messenger.Send(new RunCreatedMessaged(result));
            }, logger.Error).ConfigureAwait(false);
        }
        else
        {
            logger.Info(Resource.RangeIsNotSetMsg);
        }
    }
}

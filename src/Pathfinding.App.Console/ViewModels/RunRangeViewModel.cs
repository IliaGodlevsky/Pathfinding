using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Extensions;
using Pathfinding.Shared.Extensions;
using ReactiveUI;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

// ReSharper disable AsyncVoidLambda
// ReSharper disable once UnusedMember.Global

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunRangeViewModel : ViewModel,
    IPathfindingRange<GraphVertexModel>, IRunRangeViewModel, IDisposable
{
    private readonly CompositeDisposable disposables = [];
    private readonly CompositeDisposable shortLifeDisposables = [];
    private readonly IRangeRequestService<GraphVertexModel> rangeService;
    private readonly ReadOnlyCollection<Command> includeCommands;
    private readonly ReadOnlyCollection<Command> excludeCommands;

    private ActiveGraph activeGraph;
    private ActiveGraph ActivatedGraph
    {
        get => activeGraph;
        set => this.RaiseAndSetIfChanged(ref activeGraph, value);
    }

    private GraphVertexModel source;
    public GraphVertexModel Source
    {
        get => source;
        set
        {
            var previous = source;
            source = value;
            if (value == null && previous != null)
            {
                previous.IsSource = false;
            }
            else if (value != null)
            {
                value.IsSource = true;
            }
            this.RaisePropertyChanged();
        }
    }

    private GraphVertexModel target;
    public GraphVertexModel Target
    {
        get => target;
        set
        {
            var previous = target;
            target = value;
            if (value == null && previous != null)
            {
                previous.IsTarget = false;
            }
            else if (value != null)
            {
                value.IsTarget = true;
            }
            this.RaisePropertyChanged();
        }
    }

    public ObservableCollection<GraphVertexModel> Transit { get; } = [];

    IList<GraphVertexModel> IPathfindingRange<GraphVertexModel>.Transit => Transit;

    public ReactiveCommand<GraphVertexModel, Unit> AddToRangeCommand { get; }

    public ReactiveCommand<GraphVertexModel, Unit> RemoveFromRangeCommand { get; }

    public ReactiveCommand<Unit, Unit> DeletePathfindingRange { get; }

    public RunRangeViewModel(
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        [KeyFilter(KeyFilters.IncludeCommands)] Meta<Command>[] includeCommands,
        [KeyFilter(KeyFilters.ExcludeCommands)] Meta<Command>[] excludeCommands,
        IRangeRequestService<GraphVertexModel> rangeService,
        ILog logger) : base(logger)
    {
        this.rangeService = rangeService;
        this.includeCommands = includeCommands
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => x.Value)
            .ToList().AsReadOnly();
        this.excludeCommands = excludeCommands
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => x.Value)
            .ToList().AsReadOnly();
        messenger.RegisterHandler<IsVertexInRangeRequestMessage>(this, OnVertexIsInRangeReceived).DisposeWith(disposables);
        messenger.RegisterHandler<PathfindingRangeRequestMessage>(this, OnGetPathfindingRangeReceived).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
        messenger.RegisterHandler<GraphStateChangedMessage>(this, OnGraphBecameReadonly).DisposeWith(disposables);
        messenger.RegisterAwaitHandler<AwaitGraphActivatedMessage>(this, OnGraphFieldActivated).DisposeWith(disposables);
        AddToRangeCommand = ReactiveCommand.Create<GraphVertexModel>(AddVertexToRange, CanExecute()).DisposeWith(disposables);
        RemoveFromRangeCommand = ReactiveCommand.Create<GraphVertexModel>(RemoveVertexFromRange, CanExecute()).DisposeWith(disposables);
        DeletePathfindingRange = ReactiveCommand.CreateFromTask(DeleteRange, CanExecute()).DisposeWith(disposables);
        shortLifeDisposables.DisposeWith(disposables);
    }

    private IObservable<bool> CanExecute()
    {
        return this.WhenAnyValue(x => x.ActivatedGraph, g => !g.IsReadonly);
    }

    public IEnumerator<GraphVertexModel> GetEnumerator()
    {
        return Transit.Prepend(Source).Append(Target).GetEnumerator();
    }

    private void AddVertexToRange(GraphVertexModel vertex)
    {
        includeCommands.ExecuteFirst(this, vertex);
    }

    private void BindTo(Expression<Func<GraphVertexModel, bool>> expression,
        GraphVertexModel model)
    {
        model.WhenAnyValue(expression).Skip(1).Where(x => x)
            .Subscribe(async _ => await AddRangeToStorage(model))
            .DisposeWith(shortLifeDisposables);
        model.WhenAnyValue(expression).Skip(1).Where(x => !x)
            .Subscribe(async _ => await RemoveVertexFromStorage(model))
            .DisposeWith(shortLifeDisposables);
    }

    private async Task AddRangeToStorage(GraphVertexModel vertex)
    {
        await ExecuteSafe(async token =>
        {
            var vertices = this.ToList();
            var index = vertices.IndexOf(vertex);
            await rangeService.CreatePathfindingVertexAsync(ActivatedGraph.Id,
                vertex.Id, index, token).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private void RemoveVertexFromRange(GraphVertexModel vertex)
    {
        excludeCommands.ExecuteFirst(this, vertex);
    }

    private async Task RemoveVertexFromStorage(GraphVertexModel vertex)
    {
        await ExecuteSafe(async token =>
        {
            await rangeService.DeleteRangeAsync(vertex.Enumerate(), token).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private void ClearRange()
    {
        Source = null;
        Target = null;
        while (Transit.Count > 0) Transit.RemoveAt(0);
    }

    private async Task DeleteRange()
    {
        var result = await rangeService.DeleteRangeAsync(ActivatedGraph.Id).ConfigureAwait(false);
        if (result)
        {
            ClearRange();
        }
    }

    private async Task OnGraphFieldActivated(AwaitGraphActivatedMessage msg)
    {
        await ExecuteSafe(async token =>
        {
            shortLifeDisposables.Clear();
            Transit.CollectionChanged -= OnCollectionChanged;
            ClearRange();
            ActivatedGraph = msg.Value.ActiveGraph;
            var range = await rangeService.ReadRangeAsync(ActivatedGraph.Id, token).ConfigureAwait(false);
            var src = range.FirstOrDefault(x => x.IsSource);
            Source = src != null ? ActivatedGraph.VertexMap[src.VertexId] : null;
            var tgt = range.FirstOrDefault(x => x.IsTarget);
            Target = tgt != null ? ActivatedGraph.VertexMap[tgt.VertexId] : null;
            var transit = range.Where(x => !x.IsSource && !x.IsTarget)
                .Select(x => ActivatedGraph.VertexMap[x.VertexId])
                .ToList();
            Transit.CollectionChanged += OnCollectionChanged;
            Transit.AddRange(transit);
            foreach (var vertex in ActivatedGraph.Graph)
            {
                BindTo(x => x.IsSource, vertex);
                BindTo(x => x.IsTarget, vertex);
                BindTo(x => x.IsTransit, vertex);
            }
        }).ConfigureAwait(false);
    }

    private void OnGraphBecameReadonly(GraphStateChangedMessage msg)
    {
        bool isReadonly = msg.Value.Status == GraphStatuses.Readonly;
        ActivatedGraph = new ActiveGraph(ActivatedGraph.Id, ActivatedGraph.Graph, isReadonly);
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraph.Id))
        {
            shortLifeDisposables.Clear();
            ClearRange();
            ActivatedGraph = ActiveGraph.Empty;
        }
    }

    private static void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var added = (GraphVertexModel)e.NewItems?[0];
                if (added != null) added.IsTransit = true;
                break;
            case NotifyCollectionChangedAction.Remove:
                var removed = (GraphVertexModel)e.OldItems?[0];
                if (removed != null) removed.IsTransit = false;
                break;
        }
    }

    private void OnVertexIsInRangeReceived(IsVertexInRangeRequestMessage request)
    {
        var contains = this.Contains(request.Vertex);
        request.Reply(contains);
    }

    private void OnGetPathfindingRangeReceived(PathfindingRangeRequestMessage msg)
    {
        var range = this
            .Where(x => x is not null)
            .ToArray();
        msg.Reply(range);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        disposables.Dispose();
        Transit.CollectionChanged -= OnCollectionChanged;
    }
}
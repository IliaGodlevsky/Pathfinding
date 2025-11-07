using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Pathfinding;
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
    private readonly IEnumerable<Command> includeCommands;
    private readonly IEnumerable<Command> excludeCommands;
    private readonly IPathfindingRange<GraphVertexModel> pathfindingRange;

    private int GraphId { get; set; }

    private Graph<GraphVertexModel> Graph { get; set; }

    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => this.RaiseAndSetIfChanged(ref isReadOnly, value);
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
        pathfindingRange = this;
        this.rangeService = rangeService;
        this.includeCommands = includeCommands
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => x.Value)
            .ToReadOnly();
        this.excludeCommands = excludeCommands
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => x.Value)
            .ToReadOnly();
        messenger.RegisterHandler<IsVertexInRangeRequestMessage>(this, OnVertexIsInRangeReceived).DisposeWith(disposables);
        messenger.RegisterHandler<PathfindingRangeRequestMessage>(this, OnGetPathfindingRangeReceived).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
        messenger.RegisterHandler<GraphStateChangedMessage>(this, OnGraphBecameReadonly).DisposeWith(disposables);
        messenger.RegisterAwaitHandler<AwaitGraphActivatedMessage, int>(this, Tokens.PathfindingRange, OnGraphActivated).DisposeWith(disposables);
        AddToRangeCommand = ReactiveCommand.Create<GraphVertexModel>(AddVertexToRange, CanExecute()).DisposeWith(disposables);
        RemoveFromRangeCommand = ReactiveCommand.Create<GraphVertexModel>(RemoveVertexFromRange, CanExecute()).DisposeWith(disposables);
        DeletePathfindingRange = ReactiveCommand.CreateFromTask(DeleteRange, CanExecute()).DisposeWith(disposables);
        shortLifeDisposables.DisposeWith(disposables);
    }

    private IObservable<bool> CanExecute()
    {
        return this.WhenAnyValue(x => x.IsReadOnly, isRead => !isRead);
    }

    public IEnumerator<GraphVertexModel> GetEnumerator()
    {
        return Transit.Prepend(Source).Append(Target).GetEnumerator();
    }

    private void AddVertexToRange(GraphVertexModel vertex)
    {
        includeCommands.ExecuteFirst(pathfindingRange, vertex);
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
            var vertices = pathfindingRange.ToList();
            var index = vertices.IndexOf(vertex);
            await rangeService.CreatePathfindingVertexAsync(GraphId,
                vertex.Id, index, token).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private void RemoveVertexFromRange(GraphVertexModel vertex)
    {
        excludeCommands.ExecuteFirst(pathfindingRange, vertex);
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
        var result = await rangeService.DeleteRangeAsync(GraphId).ConfigureAwait(false);
        if (result)
        {
            ClearRange();
        }
    }

    private async Task OnGraphActivated(AwaitGraphActivatedMessage msg)
    {
        await ExecuteSafe(async token =>
        {
            shortLifeDisposables.Clear();
            Transit.CollectionChanged -= OnCollectionChanged;
            ClearRange();
            Graph = msg.Value.Graph;
            GraphId = msg.Value.GraphId;
            IsReadOnly = msg.Value.Status == GraphStatuses.Readonly;
            var range = await rangeService.ReadRangeAsync(GraphId, token).ConfigureAwait(false);
            var src = range.FirstOrDefault(x => x.IsSource);
            Source = src != null ? Graph.Get(src.Position) : null;
            var tgt = range.FirstOrDefault(x => x.IsTarget);
            Target = tgt != null ? Graph.Get(tgt.Position) : null;
            var transit = range.Where(x => !x.IsSource && !x.IsTarget)
                .Select(x => Graph.Get(x.Position))
                .ToList();
            Transit.CollectionChanged += OnCollectionChanged;
            Transit.AddRange(transit);
            foreach (var vertex in Graph)
            {
                BindTo(x => x.IsSource, vertex);
                BindTo(x => x.IsTarget, vertex);
                BindTo(x => x.IsTransit, vertex);
            }
        }).ConfigureAwait(false);
    }

    private void OnGraphBecameReadonly(GraphStateChangedMessage msg)
    {
        IsReadOnly = msg.Value.Status == GraphStatuses.Readonly;
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(GraphId))
        {
            disposables.Clear();
            ClearRange();
            GraphId = 0;
            IsReadOnly = false;
            Graph = Graph<GraphVertexModel>.Empty;
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
        var contains = pathfindingRange.Contains(request.Vertex);
        request.Reply(contains);
    }

    private void OnGetPathfindingRangeReceived(PathfindingRangeRequestMessage msg)
    {
        var range = pathfindingRange
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
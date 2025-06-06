﻿using Autofac.Features.AttributeFilters;
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

namespace Pathfinding.App.Console.ViewModels;

// ReSharper disable once UnusedMember.Global
internal sealed class RunRangeViewModel : BaseViewModel,
    IPathfindingRange<GraphVertexModel>, IRunRangeViewModel
{
    private readonly CompositeDisposable disposables = [];
    private readonly IRequestService<GraphVertexModel> service;
    private readonly IEnumerable<Command> includeCommands;
    private readonly IEnumerable<Command> excludeCommands;
    private readonly IPathfindingRange<GraphVertexModel> pathfindingRange;
    private readonly ILog logger;

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
        IRequestService<GraphVertexModel> service,
        ILog logger)
    {
        pathfindingRange = this;
        this.service = service;
        this.includeCommands = includeCommands
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => x.Value)
            .ToReadOnly();
        this.excludeCommands = excludeCommands
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => x.Value)
            .ToReadOnly();
        this.logger = logger;
        messenger.Register<IsVertexInRangeRequestMessage>(this, OnVertexIsInRangeReceived);
        messenger.Register<PathfindingRangeRequestMessage>(this, OnGetPathfindingRangeReceived);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
        messenger.Register<GraphStateChangedMessage>(this, OnGraphBecameReadonly);
        messenger.RegisterAwaitHandler<AwaitGraphActivatedMessage, int>(this, Tokens.PathfindingRange, OnGraphActivated);
        AddToRangeCommand = ReactiveCommand.Create<GraphVertexModel>(AddVertexToRange, CanExecute());
        RemoveFromRangeCommand = ReactiveCommand.Create<GraphVertexModel>(RemoveVertexFromRange, CanExecute());
        DeletePathfindingRange = ReactiveCommand.CreateFromTask(DeleteRange, CanExecute());
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
            .Do(async _ => await AddRangeToStorage(model))
            .Subscribe().DisposeWith(disposables);
        model.WhenAnyValue(expression).Skip(1).Where(x => !x)
            .Do(async _ => await RemoveVertexFromStorage(model))
            .Subscribe().DisposeWith(disposables);
    }

    private void SubscribeToEvents(GraphVertexModel vertex)
    {
        BindTo(x => x.IsSource, vertex);
        BindTo(x => x.IsTarget, vertex);
        BindTo(x => x.IsTransit, vertex);
    }

    private async Task AddRangeToStorage(GraphVertexModel vertex)
    {
        await ExecuteSafe(async () =>
        {
            var vertices = pathfindingRange.ToList();
            var index = vertices.IndexOf(vertex);
            await service.CreatePathfindingVertexAsync(GraphId, vertex.Id, index)
                .ConfigureAwait(false);
        }, logger.Error).ConfigureAwait(false);
    }

    private void RemoveVertexFromRange(GraphVertexModel vertex)
    {
        excludeCommands.ExecuteFirst(pathfindingRange, vertex);
    }

    private async Task RemoveVertexFromStorage(GraphVertexModel vertex)
    {
        await ExecuteSafe(async ()
            => await service.DeleteRangeAsync(vertex.Enumerate()).ConfigureAwait(false),
            logger.Error).ConfigureAwait(false);
    }

    private void ClearRange()
    {
        Source = null;
        Target = null;
        while (Transit.Count > 0) Transit.RemoveAt(0);
    }

    private async Task DeleteRange()
    {
        var result = await service.DeleteRangeAsync(GraphId).ConfigureAwait(false);
        if (result)
        {
            ClearRange();
        }
    }

    private async Task OnGraphActivated(object recipient, AwaitGraphActivatedMessage msg)
    {
        await ExecuteSafe(async () =>
        {
            disposables.Clear();
            Transit.CollectionChanged -= OnCollectionChanged;
            ClearRange();
            Graph = msg.Value.Graph;
            GraphId = msg.Value.GraphId;
            IsReadOnly = msg.Value.Status == GraphStatuses.Readonly;
            var range = await service.ReadRangeAsync(GraphId).ConfigureAwait(false);
            var src = range.FirstOrDefault(x => x.IsSource);
            Source = src != null ? Graph.Get(src.Position) : null;
            var tgt = range.FirstOrDefault(x => x.IsTarget);
            Target = tgt != null ? Graph.Get(tgt.Position) : null;
            var transit = range.Where(x => !x.IsSource && !x.IsTarget)
                .Select(x => Graph.Get(x.Position))
                .ToList();
            Transit.CollectionChanged += OnCollectionChanged;
            Transit.AddRange(transit);
            Graph.ForEach(SubscribeToEvents);
        }, logger.Error).ConfigureAwait(false);
    }

    private void OnGraphBecameReadonly(object recipient, GraphStateChangedMessage msg)
    {
        IsReadOnly = msg.Value.Status == GraphStatuses.Readonly;
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
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

    private void OnVertexIsInRangeReceived(object recipient, IsVertexInRangeRequestMessage request)
    {
        var contains = pathfindingRange.Contains(request.Vertex);
        request.Reply(contains);
    }

    private void OnGetPathfindingRangeReceived(object recipient, PathfindingRangeRequestMessage msg)
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
}
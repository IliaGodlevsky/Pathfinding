﻿using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;
using Pathfinding.App.Console.Factories;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphUpdateViewModel : BaseViewModel
{
    private readonly IMessenger messenger;
    private readonly INeighborhoodLayerFactory neighborFactory;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly ILog log;

    private GraphInfoModel[] selectedGraph = [];
    public GraphInfoModel[] SelectedGraphs
    {
        get => selectedGraph;
        set => this.RaiseAndSetIfChanged(ref selectedGraph, value);
    }

    private Neighborhoods neighborhood;
    public Neighborhoods Neighborhood
    {
        get => neighborhood;
        set => this.RaiseAndSetIfChanged(ref neighborhood, value);
    }

    public IReadOnlyCollection<Neighborhoods> Allowed 
        => neighborFactory.Allowed;

    private string name;
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    private GraphStatuses status;
    public GraphStatuses Status
    {
        get => status;
        set => this.RaiseAndSetIfChanged(ref status, value);
    }

    public ReactiveCommand<Unit, Unit> UpdateGraphCommand { get; }

    public GraphUpdateViewModel(IRequestService<GraphVertexModel> service,
        INeighborhoodLayerFactory neighborFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog log)
    {
        this.messenger = messenger;
        this.service = service;
        this.log = log;
        this.neighborFactory = neighborFactory;
        messenger.Register<GraphsSelectedMessage>(this, OnGraphSelected);
        messenger.Register<GraphStateChangedMessage>(this, OnStatusChanged);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
        UpdateGraphCommand = ReactiveCommand.CreateFromTask(ExecuteUpdate, CanExecute());
    }

    private IObservable<bool> CanExecute()
    {
        return this.WhenAnyValue(
            x => x.SelectedGraphs,
            x => x.Name,
            (selected, x)
                => selected.Length == 1 && !string.IsNullOrEmpty(x));
    }

    private async Task ExecuteUpdate()
    {
        await ExecuteSafe(async () =>
        {
            var graph = SelectedGraphs[0];
            var info = await service.ReadGraphInfoAsync(graph.Id)
                .ConfigureAwait(false);
            info.Name = Name;
            info.Neighborhood = Neighborhood;
            await service.UpdateGraphInfoAsync(info).ConfigureAwait(false);
            await messenger.Send(new AwaitGraphUpdatedMessage(info), Tokens.GraphTable);
            messenger.Send(new GraphUpdatedMessage(info));
            await messenger.Send(new AwaitGraphUpdatedMessage(info), Tokens.AlgorithmUpdate);
        }, log.Error).ConfigureAwait(false);
    }

    private void OnStatusChanged(object recipient, GraphStateChangedMessage msg)
    {
        Status = msg.Value.Status;
    }

    private void OnGraphSelected(object recipient, GraphsSelectedMessage msg)
    {
        if (msg.Value.Length == 1)
        {
            SelectedGraphs = msg.Value;
            var graph = SelectedGraphs[0];
            Name = graph.Name;
            Neighborhood = graph.Neighborhood;
            Status = graph.Status;
        }
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        SelectedGraphs = [.. SelectedGraphs
            .Where(x => !msg.Value.Contains(x.Id))];
        if (SelectedGraphs.Length == 0)
        {
            Name = string.Empty;
        }
    }
}

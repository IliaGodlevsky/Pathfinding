﻿using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;
// ReSharper disable PossibleInvalidOperationException

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphImportViewModel : BaseViewModel, IGraphImportViewModel
{
    private readonly Dictionary<StreamFormat, Serializer> serializers;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly IMessenger messenger;

    public ReactiveCommand<Func<StreamModel>, Unit> ImportGraphCommand { get; }

    public IReadOnlyCollection<StreamFormat> StreamFormats { get; }

    public GraphImportViewModel(IRequestService<GraphVertexModel> service,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        Meta<Serializer>[] serializers,
        ILog logger) : base(logger)
    {
        this.messenger = messenger;
        this.service = service;
        this.serializers = serializers.ToDictionary(x =>
            (StreamFormat)x.Metadata[MetadataKeys.ExportFormat], x => x.Value);
        StreamFormats = [.. serializers
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => (StreamFormat)x.Metadata[MetadataKeys.ExportFormat])];
        ImportGraphCommand = ReactiveCommand.CreateFromTask<Func<StreamModel>>(ImportGraphs);
    }

    private async Task ImportGraphs(Func<StreamModel> streamFactory)
    {
        await ExecuteSafe(async () =>
        {
            await using var stream = streamFactory();
            if (!stream.IsEmpty)
            {
                var serializer = serializers[stream.Format.Value];
                var histories = await serializer
                    .DeserializeFromAsync(stream.Stream, CancellationToken.None)
                    .ConfigureAwait(false);
                var timeout = Timeout * histories.Histories.Count;
                using var cts = new CancellationTokenSource(timeout);
                var result = await service.CreatePathfindingHistoriesAsync(
                    histories.Histories, cts.Token).ConfigureAwait(false);
                var graphs = result.Select(x => x.Graph).ToGraphInfo();
                messenger.Send(new GraphsCreatedMessage(graphs));
                log.Info(graphs.Length > 0
                    ? Resource.WasLoadedMsg
                    : Resource.WereLoadedMsg);
            }
        }).ConfigureAwait(false);
    }
}

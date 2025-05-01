using Autofac.Features.AttributeFilters;
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
    private readonly ILog logger;

    public ReactiveCommand<StreamModel, Unit> ImportGraphCommand { get; }

    public IReadOnlyCollection<StreamFormat> StreamFormats { get; }

    public GraphImportViewModel(IRequestService<GraphVertexModel> service,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        Meta<Serializer>[] serializers,
        ILog logger)
    {
        this.messenger = messenger;
        this.service = service;
        this.logger = logger;
        this.serializers = serializers.ToDictionary(x =>
            (StreamFormat)x.Metadata[MetadataKeys.ExportFormat], x => x.Value);
        StreamFormats = [.. serializers
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => (StreamFormat)x.Metadata[MetadataKeys.ExportFormat])];
        ImportGraphCommand = ReactiveCommand.CreateFromTask<StreamModel>(ImportGraphs);
    }

    private async Task ImportGraphs(StreamModel stream)
    {
        await ExecuteSafe(async () =>
        {
            await using (stream)
            {
                if (!stream.IsEmpty)
                {
                    var serializer = serializers[stream.Format.Value];
                    var histories = await serializer.DeserializeFromAsync(stream.Stream)
                        .ConfigureAwait(false);
                    var result = await service.CreatePathfindingHistoriesAsync(histories.Histories)
                        .ConfigureAwait(false);
                    var graphs = result.Select(x => x.Graph).ToGraphInfo();
                    messenger.Send(new GraphsCreatedMessage(graphs));
                    logger.Info(graphs.Length > 0
                        ? Resource.WasLoadedMsg
                        : Resource.WereLoadedMsg);
                }
            }
        }, logger.Error).ConfigureAwait(false);
    }
}

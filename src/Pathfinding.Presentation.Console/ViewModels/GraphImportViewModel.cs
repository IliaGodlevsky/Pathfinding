using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Logging.Interface;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Factories;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.Resources;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using Pathfinding.Serialization.Decorators;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Serialization;
using ReactiveUI;
using System.Reactive;
// ReSharper disable PossibleInvalidOperationException

namespace Pathfinding.Presentation.Console.ViewModels;

internal sealed class GraphImportViewModel : ViewModel, IGraphImportViewModel
{
    private readonly ISerializerFactory serializerFactory;
    private readonly IGraphRequestService<GraphVertexModel> service;
    private readonly IMessenger messenger;

    public ReactiveCommand<Func<StreamModel>, Unit> ImportGraphCommand { get; }

    public IReadOnlyCollection<SerializationFormat> SerializationFormats => serializerFactory.AvailiableFormats;

    public GraphImportViewModel(IGraphRequestService<GraphVertexModel> service,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ISerializerFactory serializerFactory,
        ILog logger) : base(logger)
    {
        this.messenger = messenger;
        this.service = service;
        this.serializerFactory = serializerFactory;
        ImportGraphCommand = ReactiveCommand.CreateFromTask<Func<StreamModel>>(ImportGraphs);
    }

    private async Task ImportGraphs(Func<StreamModel> streamFactory)
    {
        await ExecuteSafe(async token =>
        {
            await using var stream = streamFactory();
            if (stream.IsEmpty) return;
            var serializer = serializerFactory.Create(stream.Format.Value);
            serializer = stream.NeedsCompress 
                ? GetCompressSerializer(serializer) 
                : serializer;
            var histories = await serializer
                .DeserializeFromAsync(stream.Stream, token)
                .ConfigureAwait(false);
            var result = await service
                .CreatePathfindingHistoriesAsync(histories.Histories, token)
                .ConfigureAwait(false);
            var graphs = result.Select(x => x.Graph).ToGraphInfo();
            messenger.Send(new GraphsCreatedMessage(graphs));
            log.Info(graphs.Length > 0
                ? Resource.WasLoadedMsg
                : Resource.WereLoadedMsg);
        }).ConfigureAwait(false);
    }

    private static CompressSerializer<PathfindingHistoriesSerializationModel> GetCompressSerializer(ISerializer<PathfindingHistoriesSerializationModel> serializer)
    {
        return new(serializer);
    }
}

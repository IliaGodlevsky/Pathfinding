using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Factories;
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

internal sealed class GraphImportViewModel : ViewModel, IGraphImportViewModel
{
    private readonly ISerializerFactory serializerFactory;
    private readonly IGraphRequestService<GraphVertexModel> service;
    private readonly IMessenger messenger;

    public ReactiveCommand<Func<StreamModel>, Unit> ImportGraphCommand { get; }

    public IReadOnlyCollection<StreamFormat> StreamFormats => serializerFactory.AvailiableFormats;

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
            var serializer = serializerFactory.CreateSerializer(stream.Format.Value);
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
}

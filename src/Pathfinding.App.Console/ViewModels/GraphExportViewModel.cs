using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;

using Serializer = Pathfinding.Service.Interface
    .ISerializer<Pathfinding.Service.Interface.Models.Serialization
        .PathfindingHisotiriesSerializationModel>;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphExportViewModel : BaseViewModel, IGraphExportViewModel
{
    private readonly IRequestService<GraphVertexModel> service;
    private readonly Dictionary<StreamFormat, Serializer> serializers;
    private readonly ILog logger;

    private int[] selectedGraphIds = [];
    private int[] SelectedGraphIds
    {
        get => selectedGraphIds;
        set => this.RaiseAndSetIfChanged(ref selectedGraphIds, value);
    }

    public ExportOptions Options { get; set; }

    public ReactiveCommand<Func<StreamModel>, Unit> ExportGraphCommand { get; }

    public IReadOnlyList<ExportOptions> AllowedOptions { get; }

    public IReadOnlyCollection<StreamFormat> StreamFormats => serializers.Keys;

    public GraphExportViewModel(IRequestService<GraphVertexModel> service,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        IEnumerable<Meta<Serializer>> serializers,
        ILog logger)
    {
        this.service = service;
        this.logger = logger;
        this.serializers = serializers
            .ToDictionary(x => (StreamFormat)x.Metadata[MetadataKeys.ExportFormat], x => x.Value);
        ExportGraphCommand = ReactiveCommand.CreateFromTask<Func<StreamModel>>(ExportGraph, CanExport());
        AllowedOptions = Enum.GetValues<ExportOptions>();
        messenger.Register<GraphSelectedMessage>(this, OnGraphSelected);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
    }

    private IObservable<bool> CanExport()
    {
        return this.WhenAnyValue(x => x.SelectedGraphIds,
            graphIds => graphIds.Length > 0);
    }

    private async Task ExportGraph(Func<StreamModel> streamFactory)
    {
        await ExecuteSafe(async () =>
        {
            var stream = streamFactory();
            var exportFormat = stream.Format;
            var exportStream = stream.Stream;
            await using (exportStream)
            {
                if (exportStream != Stream.Null && exportFormat.HasValue)
                {
                    var serializer = serializers[exportFormat.Value];
                    var historiesTask = Options switch
                    {
                        ExportOptions.GraphOnly => service.ReadSerializationGraphsAsync(SelectedGraphIds),
                        ExportOptions.WithRange => service.ReadSerializationGraphsWithRangeAsync(SelectedGraphIds),
                        ExportOptions.WithRuns => service.ReadSerializationHistoriesAsync(SelectedGraphIds),
                        _ => throw new InvalidOperationException(Resource.InvalidExportOptions)
                    };
                    var histories = await historiesTask.ConfigureAwait(false);
                    await serializer.SerializeToAsync(histories, exportStream).ConfigureAwait(false);
                    int count = histories.Histories.Count;
                    logger.Info(count == 1 ? Resource.WasDeletedMsg : Resource.WereDeletedMsg);
                }
            }
        }, logger.Error).ConfigureAwait(false);
    }

    private void OnGraphSelected(object recipient, GraphSelectedMessage msg)
    {
        SelectedGraphIds = [.. msg.Graphs.Select(x => x.Id)];
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        SelectedGraphIds = [.. SelectedGraphIds.Except(msg.GraphIds)];
    }
}

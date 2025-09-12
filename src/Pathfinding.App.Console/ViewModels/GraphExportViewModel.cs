using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Export;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Logging.Interface;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;

// ReSharper disable PossibleInvalidOperationException

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphExportViewModel 
    : BaseViewModel, IGraphExportViewModel, IDisposable
{
    private readonly IReadHistoryOptions options;
    private readonly Dictionary<StreamFormat, Serializer> serializers;
    private readonly CompositeDisposable disposables = [];

    private int[] selectedGraphIds = [];
    private int[] SelectedGraphIds
    {
        get => selectedGraphIds;
        set => this.RaiseAndSetIfChanged(ref selectedGraphIds, value);
    }

    public ExportOptions Option { get; set; }

    public IReadOnlyList<ExportOptions> AllowedOptions => options.Allowed;

    public IReadOnlyCollection<StreamFormat> StreamFormats { get; }

    public ReactiveCommand<Func<StreamModel>, Unit> ExportGraphCommand { get; }

    public GraphExportViewModel(IReadHistoryOptions options,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        Meta<Serializer>[] serializers,
        ILog logger) : base(logger)
    {
        this.options = options;
        this.serializers = serializers
            .ToDictionary(
                x => (StreamFormat)x.Metadata[MetadataKeys.ExportFormat], 
                x => x.Value);
        StreamFormats = [.. serializers
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => (StreamFormat)x.Metadata[MetadataKeys.ExportFormat])];
        ExportGraphCommand = ReactiveCommand.CreateFromTask<Func<StreamModel>>(ExportGraph, CanExport()).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsSelectedMessage>(this, OnGraphSelected).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
    }

    private IObservable<bool> CanExport()
    {
        return this.WhenAnyValue(x => x.SelectedGraphIds,
            graphIds => graphIds.Length > 0);
    }

    private async Task ExportGraph(Func<StreamModel> streamFactory)
    {
        await ExecuteSafe(async token =>
        {
            await using var stream = streamFactory();
            if (!stream.IsEmpty)
            {
                var histories = await options.ReadHistoryAsync(Option, 
                    SelectedGraphIds, token).ConfigureAwait(false);
                var serializer = serializers[stream.Format.Value];
                await serializer.SerializeToAsync(histories, 
                    stream.Stream, token).ConfigureAwait(false);
                log.Info(histories.Histories.Count == 1 
                    ? Resource.WasDeletedMsg 
                    : Resource.WereDeletedMsg);
            }
        }).ConfigureAwait(false);
    }

    private void OnGraphSelected(GraphsSelectedMessage msg)
    {
        SelectedGraphIds = [.. msg.Value.Select(x => x.Id)];
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        SelectedGraphIds = [.. SelectedGraphIds.Except(msg.Value)];
    }

    protected override TimeSpan GetTimeout()
    {
        return base.GetTimeout() * SelectedGraphIds.Length;
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}

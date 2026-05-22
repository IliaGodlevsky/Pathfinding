using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Logging.Interface;
using Pathfinding.Presentation.Console.Export;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Factories;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.Resources;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

// ReSharper disable PossibleInvalidOperationException

namespace Pathfinding.Presentation.Console.ViewModels;

internal sealed class GraphExportViewModel : ViewModel, IGraphExportViewModel, IDisposable
{
    private readonly IReadHistoryOptions options;
    private readonly ISerializerFactory serializerFactory;
    private readonly CompositeDisposable disposables = [];

    private int[] selectedGraphIds = [];
    private int[] SelectedGraphIds
    {
        get => selectedGraphIds;
        set => this.RaiseAndSetIfChanged(ref selectedGraphIds, value);
    }

    public ExportOptions Option { get; set; }

    public IReadOnlyList<ExportOptions> AvailableOptions => options.AvailableExportOptions;

    public IReadOnlyCollection<StreamFormat> StreamFormats => serializerFactory.AvailiableFormats;

    public ReactiveCommand<Func<StreamModel>, Unit> ExportGraphCommand { get; }

    public GraphExportViewModel(IReadHistoryOptions options,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ISerializerFactory serializerFactory,
        ILog logger) : base(logger)
    {
        this.options = options;
        this.serializerFactory = serializerFactory;
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
            if (stream.IsEmpty) return;
            var histories = await options
                .ReadHistoryAsync(Option, SelectedGraphIds, token)
                .ConfigureAwait(false);
            var serializer = serializerFactory.Create(stream.Format.Value);
            await serializer
                .SerializeToAsync(histories, stream.Stream, token)
                .ConfigureAwait(false);
            log.Info(histories.Histories.Count == 1
                ? Resource.WasDeletedMsg
                : Resource.WereDeletedMsg);
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

    public void Dispose()
    {
        disposables.Dispose();
    }
}

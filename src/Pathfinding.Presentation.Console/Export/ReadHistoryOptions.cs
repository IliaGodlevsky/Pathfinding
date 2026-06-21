using Autofac.Features.Metadata;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Serialization.Models;
using System.Collections.Frozen;

namespace Pathfinding.Presentation.Console.Export;

internal sealed class ReadHistoryOptions : IReadHistoryOptions
{
    private readonly IReadOnlyDictionary<ExportOptions, IReadHistoryOption> options;

    public IReadOnlyList<ExportOptions> AvailableExportOptions { get; }

    public ReadHistoryOptions(Meta<IReadHistoryOption>[] options)
    {
        this.options = options.ToFrozenDictionary(
            x => (ExportOptions)x.Metadata[MetadataKeys.ExportOptions],
            x => x.Value);
        AvailableExportOptions = [.. this.options.Keys];
    }

    public Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        ExportOptions option,
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        if (options.TryGetValue(option, out var value))
        {
            return value.ReadHistoryAsync(graphIds, token);
        }

        throw new KeyNotFoundException($"{option} was not found");
    }
}
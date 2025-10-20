using CsvHelper;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Serialization.Csv;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace Pathfinding.Service.Interface.Extensions;

internal static class ZipArchiveExtensions
{
    private const string Graphs = "graphs.csv";
    private const string Vertices = "vertices.csv";
    private const string Statistics = "statistics.csv";
    private const string Ranges = "ranges.csv";

    private static async Task<T[]> ReadEntryAsync<T>(this ZipArchive archive,
        string entryName, CancellationToken token = default)
    {
        var entry = archive.GetEntry(entryName);
        if (entry is not null)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream,
                Encoding.UTF8, leaveOpen: true, bufferSize: 1024);
            using var csvReader = new CsvReader(reader,
                CultureInfo.InvariantCulture, leaveOpen: true);
            return await csvReader.GetRecordsAsync<T>(token)
                .ToArrayAsync(token).ConfigureAwait(false);
        }
        return [];
    }

    private static async Task WriteEntryAsync<T>(this ZipArchive archive,
        string entryName, IEnumerable<T> records,
        CancellationToken token = default)
    {
        var toWrite = records.ToList();
        if (toWrite.Count > 0)
        {
            var entry = archive.CreateEntry(entryName);
            await using var entryStream = entry.Open();
            await using var writer = new StreamWriter(entryStream,
                Encoding.UTF8, leaveOpen: true, bufferSize: 1024);
            await using var csvWriter = new CsvWriter(writer,
                CultureInfo.InvariantCulture, leaveOpen: true);
            await csvWriter.WriteRecordsAsync(toWrite, token)
                .ConfigureAwait(false);
        }
    }

    public static async Task WriteHistoryAsync(this ZipArchive archive,
        IEnumerable<PathfindingHistorySerializationModel> model,
        CancellationToken token = default)
    {
        var records = model.ToHistory();
        await archive.WriteEntryAsync(Graphs, records.Select(x => x.Graph), token).ConfigureAwait(false);
        await archive.WriteEntryAsync(Vertices, records.SelectMany(x => x.Vertices), token).ConfigureAwait(false);
        await archive.WriteEntryAsync(Statistics, records.SelectMany(x => x.Statistics), token).ConfigureAwait(false);
        await archive.WriteEntryAsync(Ranges, records.SelectMany(x => x.Range), token).ConfigureAwait(false);
    }

    public static async Task<List<PathfindingHistorySerializationModel>> ReadHistoryAsync(this ZipArchive archive,
        CancellationToken token = default)
    {
        var graphs = await archive.ReadEntryAsync<CsvGraph>(Graphs, token).ConfigureAwait(false);
        var vertices = await archive.ReadEntryAsync<CsvVertex>(Vertices, token).ConfigureAwait(false);
        var statistics = await archive.ReadEntryAsync<CsvStatistics>(Statistics, token).ConfigureAwait(false);
        var ranges = await archive.ReadEntryAsync<CsvRange>(Ranges, token).ConfigureAwait(false);
        return (graphs, vertices, statistics, ranges).ToHistory();
    }
}
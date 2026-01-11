using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TmdbBoxsetMetadataForCollections.Services;

public class CollectionScanner
{
    private readonly ILibraryManager _libraryManager;
    private readonly TmdbBoxsetClient _tmdb;
    private readonly CollectionMetadataWriter _writer;
    private readonly ILogger<CollectionScanner> _logger;

    public CollectionScanner(
        ILibraryManager libraryManager,
        TmdbBoxsetClient tmdb,
        CollectionMetadataWriter writer,
        ILogger<CollectionScanner> logger)
    {
        _libraryManager = libraryManager;
        _tmdb = tmdb;
        _writer = writer;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var collections = _libraryManager.GetItemList(new MediaBrowser.Controller.Entities.InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.Collection }
        });

        foreach (var col in collections)
        {
            var movie = col.GetChildren().OfType<Movie>().FirstOrDefault();
            if (movie == null) continue;

            if (!movie.ProviderIds.TryGetValue("TmdbCollection", out var boxsetId)) continue;

            _logger.LogInformation("[CTB] {0} -> TMDb {1}", col.Name, boxsetId);

            var boxset = await _tmdb.GetBoxsetAsync(boxsetId, ct);
            if (boxset == null) continue;

            await _writer.ApplyAsync(col, boxset, ct);
        }
    }
}

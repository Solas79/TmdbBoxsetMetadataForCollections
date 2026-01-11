using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

public class TmdbCollectionMetadataProvider :
    IMetadataProvider<CollectionFolder>,
    IRemoteMetadataProvider<CollectionFolder, CollectionInfo>
{
    private readonly ITmdbClient _tmdb;
    private readonly ILogger<TmdbCollectionMetadataProvider> _logger;

    public TmdbCollectionMetadataProvider(ITmdbClient tmdb, ILogger<TmdbCollectionMetadataProvider> logger)
    {
        _tmdb = tmdb;
        _logger = logger;
    }

    public string Name => "TMDb Boxset Metadata For Collections";

    public async Task<MetadataResult<CollectionFolder>> GetMetadata(
        CollectionInfo info,
        CancellationToken cancellationToken)
    {
        var result = new MetadataResult<CollectionFolder>();

        if (info.Item == null)
            return result;

        var collection = info.Item;

        if (collection.ProviderIds.ContainsKey(MetadataProvider.TmdbCollection))
            return result; // already has ID

        var movies = collection.GetRecursiveChildren()
            .OfType<Movie>()
            .ToList();

        var tmdbCollectionId = movies
            .Select(m => m.GetProviderId(MetadataProvider.TmdbCollection))
            .FirstOrDefault(id => !string.IsNullOrEmpty(id));

        if (tmdbCollectionId == null)
        {
            _logger.LogDebug("Collection {Name} has no movies with TMDb collection ID", collection.Name);
            return result;
        }

        _logger.LogInformation("Applying TMDb Boxset {Id} to Collection {Name}", tmdbCollectionId, collection.Name);

        var tmdbCollection = await _tmdb.GetCollectionAsync(tmdbCollectionId, cancellationToken);

        if (tmdbCollection == null)
            return result;

        collection.SetProviderId(MetadataProvider.TmdbCollection, tmdbCollectionId);

        collection.Overview = tmdbCollection.Overview;
        collection.Name = tmdbCollection.Name;

        result.HasMetadata = true;
        result.Item = collection;

        return result;
    }

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        CollectionInfo searchInfo,
        CancellationToken cancellationToken)
        => Task.FromResult(Enumerable.Empty<RemoteSearchResult>());

    public Task<RemoteMetadataResult<CollectionFolder>> GetMetadata(
        string id,
        CancellationToken cancellationToken)
        => Task.FromResult(new RemoteMetadataResult<CollectionFolder>());
}

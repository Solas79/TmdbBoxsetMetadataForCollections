using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public class TmdbCollectionMetadataProvider : IMetadataProvider<CollectionFolder>
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<TmdbCollectionMetadataProvider> _logger;

        public TmdbCollectionMetadataProvider(
            ILibraryManager libraryManager,
            ILogger<TmdbCollectionMetadataProvider> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public string Name => "TMDb Boxset Metadata Provider for Collections";

        public async Task<MetadataResult<CollectionFolder>> GetMetadata(
            CollectionFolder collection,
            CancellationToken cancellationToken)
        {
            // Wenn die Collection bereits eine TMDb Boxset ID hat → nichts tun
            if (!string.IsNullOrWhiteSpace(collection.GetProviderId(MetadataProvider.TmdbCollection)))
                return new MetadataResult<CollectionFolder> { Item = collection };

            _logger.LogInformation("Scanning collection: {Name}", collection.Name);

            var movies = _libraryManager.GetItemList(new InternalItemsQuery
            {
                ParentId = collection.Id,
                IncludeItemTypes = new[] { BaseItemKind.Movie }, // Jellyfin 10.11: Enum statt string[]
                Recursive = true
            }).OfType<Movie>().ToList();

            if (movies.Count == 0)
            {
                _logger.LogInformation("Collection has no movies: {Name}", collection.Name);
                return new MetadataResult<CollectionFolder> { Item = collection };
            }

            var tmdbCollectionId = movies
                .Select(m => m.GetProviderId(MetadataProvider.TmdbCollection))
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

            if (tmdbCollectionId == null)
            {
                _logger.LogInformation("No TMDbCollectionId found for {Name}", collection.Name);
                return new MetadataResult<CollectionFolder> { Item = collection };
            }

            _logger.LogInformation("Applying TMDbCollectionId {Id} to {Name}", tmdbCollectionId, collection.Name);

            // ProviderId auf die Collection setzen (Jellyfin kann danach Boxset-Metadaten ziehen)
            collection.SetProviderId(MetadataProvider.TmdbCollection, tmdbCollectionId);

            return new MetadataResult<CollectionFolder>
            {
                Item = collection,
                HasMetadata = true
            };
        }
    }
}

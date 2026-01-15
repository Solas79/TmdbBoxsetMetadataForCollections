using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    /// <summary>
    /// Runs when Jellyfin refreshes metadata for a BoxSet (collection).
    /// If the BoxSet has no TMDbCollection id, it copies the first one found in contained movies.
    /// </summary>
    public sealed class BoxSetTmdbCollectionIdProvider : IMetadataProvider<BoxSet>
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<BoxSetTmdbCollectionIdProvider> _logger;

        public BoxSetTmdbCollectionIdProvider(
            ILibraryManager libraryManager,
            ILogger<BoxSetTmdbCollectionIdProvider> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public string Name => "TMDbCollectionId from Movies (BoxSet)";

        public Task<MetadataResult<BoxSet>> GetMetadata(BoxSet item, CancellationToken cancellationToken)
        {
            // If already set: nothing to do
            var existing = item.GetProviderId(MetadataProvider.TmdbCollection);
            if (!string.IsNullOrWhiteSpace(existing))
            {
                _logger.LogDebug("[TBMFC] BoxSet '{Name}' already has TMDbCollectionId={Id}", item.Name, existing);
                return Task.FromResult(new MetadataResult<BoxSet> { Item = item, HasMetadata = false });
            }

            // Find movies in this BoxSet
            var movies = _libraryManager.GetItemList(new InternalItemsQuery
            {
                ParentId = item.Id,
                Recursive = true,
                IncludeItemTypes = new[] { BaseItemKind.Movie }
            }).OfType<Movie>();

            var tmdbCollectionId = movies
                .Select(m => m.GetProviderId(MetadataProvider.TmdbCollection))
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

            if (string.IsNullOrWhiteSpace(tmdbCollectionId))
            {
                _logger.LogInformation("[TBMFC] No TMDbCollectionId found in movies for BoxSet '{Name}'", item.Name);
                return Task.FromResult(new MetadataResult<BoxSet> { Item = item, HasMetadata = false });
            }

            item.SetProviderId(MetadataProvider.TmdbCollection, tmdbCollectionId);
            _logger.LogInformation("[TBMFC] Set TMDbCollectionId={Id} on BoxSet '{Name}'", tmdbCollectionId, item.Name);

            return Task.FromResult(new MetadataResult<BoxSet>
            {
                Item = item,
                HasMetadata = true
            });
        }
    }
}

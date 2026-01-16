using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class BoxSetTmdbCollectionIdProvider : IMetadataProvider<BoxSet>
    {
        private const string TmdbCollectionKey = "TmdbCollection";

        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<BoxSetTmdbCollectionIdProvider> _logger;

        public BoxSetTmdbCollectionIdProvider(ILibraryManager libraryManager, ILogger<BoxSetTmdbCollectionIdProvider> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public string Name => "TMDbCollectionId from Movies (BoxSet)";

        public Task<MetadataResult<BoxSet>> GetMetadata(BoxSet item, CancellationToken cancellationToken)
        {
            // bereits gesetzt?
            if (TryGetProviderId(item, TmdbCollectionKey, out var existing) && !string.IsNullOrWhiteSpace(existing))
            {
                _logger.LogDebug("[TBMFC] BoxSet '{Name}' already has {Key}={Id}", item.Name, TmdbCollectionKey, existing);
                return Task.FromResult(new MetadataResult<BoxSet> { Item = item, HasMetadata = false });
            }

            // Movies im BoxSet finden
            var movies = _libraryManager.GetItemList(new InternalItemsQuery
            {
                ParentId = item.Id,
                Recursive = true,
                IncludeItemTypes = new[] { BaseItemKind.Movie }
            }).OfType<Movie>();

            var tmdbCollectionId = movies
                .Select(m => TryGetProviderId(m, TmdbCollectionKey, out var id) ? id : null)
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

            if (string.IsNullOrWhiteSpace(tmdbCollectionId))
            {
                _logger.LogInformation("[TBMFC] No {Key} found in movies for BoxSet '{Name}'", TmdbCollectionKey, item.Name);
                return Task.FromResult(new MetadataResult<BoxSet> { Item = item, HasMetadata = false });
            }

            SetProviderId(item, TmdbCollectionKey, tmdbCollectionId);
            _logger.LogInformation("[TBMFC] Set {Key}={Id} on BoxSet '{Name}'", TmdbCollectionKey, tmdbCollectionId, item.Name);

            return Task.FromResult(new MetadataResult<BoxSet> { Item = item, HasMetadata = true });
        }

        private static bool TryGetProviderId(BaseItem item, string key, out string? value)
        {
            value = null;
            if (item?.ProviderIds == null) return false;
            if (!item.ProviderIds.TryGetValue(key, out var v)) return false;
            value = v;
            return true;
        }

        private static void SetProviderId(BaseItem item, string key, string value)
        {
            item.ProviderIds[key] = value;
        }
    }
}

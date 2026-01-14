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
    public class TmdbCollectionMetadataProvider : IMetadataProvider<BoxSet>
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

        public Task<MetadataResult<BoxSet>> GetMetadata(
            BoxSet boxSet,
            CancellationToken cancellationToken)
        {
            // Wenn bereits eine TMDb-Boxset-ID gesetzt ist → nichts tun
            if (!string.IsNullOrWhiteSpace(boxSet.GetProviderId(MetadataProvider.TmdbCollection)))
            {
                _logger.LogDebug("BoxSet '{Name}' already has TMDbCollectionId", boxSet.Name);
                return Task.FromResult(new MetadataResult<BoxSet> { Item = boxSet });
            }

            _logger.LogInformation("Scanning BoxSet: {Name}", boxSet.Name);

            // Filme in der Collection suchen
            var movies = _libraryManager.GetItemList(new InternalItemsQuery
            {
                ParentId = boxSet.Id,
                IncludeItemTypes = new[] { BaseItemKind.Movie },
                Recursive = true
            }).OfType<Movie>().ToList();

            if (movies.Count == 0)
            {
                _logger.LogInformation("BoxSet '{Name}' contains no movies", boxSet.Name);
                return Task.FromResult(new MetadataResult<BoxSet> { Item = boxSet });
            }

            // Erste gefundene TMDbCollectionId aus den Filmen verwenden
            var tmdbCollectionId = movies
                .Select(m => m.GetProviderId(MetadataProvider.TmdbCollection))
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

            if (tmdbCollectionId == null)
            {
                _logger.LogInformation(
                    "No TMDbCollectionId found in movies for BoxSet '{Name}'",
                    boxSet.Name);

                return Task.FromResult(new MetadataResult<BoxSet> { Item = boxSet });
            }

            _logger.LogInformation(
                "Applying TMDbCollectionId {Id} to BoxSet '{Name}'",
                tmdbCollectionId,
                boxSet.Name);

            // ID auf das BoxSet setzen
            boxSet.SetProviderId(MetadataProvider.TmdbCollection, tmdbCollectionId);

            return Task.FromResult(new MetadataResult<BoxSet>
            {
                Item = boxSet,
                HasMetadata = true
            });
        }
    }
}

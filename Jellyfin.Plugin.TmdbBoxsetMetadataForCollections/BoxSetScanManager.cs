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
    public sealed class BoxSetScanManager
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IProviderManager _providerManager;
        private readonly ILogger<BoxSetScanManager> _logger;

        public BoxSetScanManager(
            ILibraryManager libraryManager,
            IProviderManager providerManager,
            ILogger<BoxSetScanManager> logger)
        {
            _libraryManager = libraryManager;
            _providerManager = providerManager;
            _logger = logger;
        }

        public async Task<int> ScanAsync(bool triggerRefresh, IProgress<double>? progress, CancellationToken ct)
        {
            _logger.LogInformation("[TBMFC] Scan started (triggerRefresh={Trigger})", triggerRefresh);

            var boxSets = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.BoxSet },
                Recursive = true
            }).OfType<BoxSet>().ToList();

            int changed = 0;
            int total = boxSets.Count;
            int done = 0;

            foreach (var bs in boxSets)
            {
                ct.ThrowIfCancellationRequested();

                done++;
                progress?.Report(total == 0 ? 100 : done * 100.0 / total);

                // hat BoxSet schon TmdbCollection?
                if (HasProviderId(bs, ProviderKeys.TmdbCollection))
                    continue;

                // Movies innerhalb BoxSet
                var movies = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    ParentId = bs.Id,
                    IncludeItemTypes = new[] { BaseItemKind.Movie },
                    Recursive = true
                }).OfType<Movie>();

                var tmdbColId = movies
                    .Select(m => GetProviderId(m, ProviderKeys.TmdbCollection))
                    .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

                if (string.IsNullOrWhiteSpace(tmdbColId))
                    continue;

                SetProviderId(bs, ProviderKeys.TmdbCollection, tmdbColId);
                changed++;

                _logger.LogInformation("[TBMFC] Set {Key}={Id} on BoxSet '{Name}'",
                    ProviderKeys.TmdbCollection, tmdbColId, bs.Name);

                if (triggerRefresh)
                {
                    // Minimaler Refresh: Jellyfin soll Metadaten/Bilder neu ziehen
                    var options = new MetadataRefreshOptions
                    {
                        ForceSave = true,
                        ReplaceAllMetadata = true,
                        ReplaceAllImages = true
                    };

                    await _providerManager.RefreshSingleItem(bs, options, ct).ConfigureAwait(false);
                    _logger.LogInformation("[TBMFC] Refreshed BoxSet '{Name}'", bs.Name);
                }
            }

            _logger.LogInformation("[TBMFC] Scan finished. Changed BoxSets: {Changed}", changed);
            return changed;
        }

        private static bool HasProviderId(BaseItem item, string key)
            => item.ProviderIds != null
               && item.ProviderIds.TryGetValue(key, out var v)
               && !string.IsNullOrWhiteSpace(v);

        private static string? GetProviderId(BaseItem item, string key)
        {
            if (item.ProviderIds == null) return null;
            return item.ProviderIds.TryGetValue(key, out var v) ? v : null;
        }

        private static void SetProviderId(BaseItem item, string key, string value)
        {
            item.ProviderIds[key] = value;
        }
    }
}

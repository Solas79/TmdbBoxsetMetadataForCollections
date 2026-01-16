using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class ScanTask : IScheduledTask
    {
        private const string TmdbCollectionKey = "TmdbCollection";

        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<ScanTask> _logger;

        public ScanTask(ILibraryManager libraryManager, ILogger<ScanTask> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public string Key => "TBMFC.ScanTask";
        public string Name => "Scan library for TMDb Boxset IDs (Collections)";
        public string Description => "Copies TMDb collection IDs from movies into BoxSets so metadata/images can be fetched.";
        public string Category => "TMDb Boxset Metadata for Collections";

        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[TBMFC] ScanTask started");

            var boxSets = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.BoxSet },
                Recursive = true
            }).OfType<BoxSet>().ToList();

            var total = boxSets.Count;
            var done = 0;

            foreach (var bs in boxSets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (TryGetProviderId(bs, TmdbCollectionKey, out var existing) && !string.IsNullOrWhiteSpace(existing))
                {
                    done++;
                    progress.Report(total == 0 ? 100 : done * 100.0 / total);
                    continue;
                }

                var movies = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    ParentId = bs.Id,
                    Recursive = true,
                    IncludeItemTypes = new[] { BaseItemKind.Movie }
                }).OfType<Movie>();

                var id = movies
                    .Select(m => TryGetProviderId(m, TmdbCollectionKey, out var mid) ? mid : null)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                if (!string.IsNullOrWhiteSpace(id))
                {
                    SetProviderId(bs, TmdbCollectionKey, id);
                    _logger.LogInformation("[TBMFC] Set {Key}={Id} on BoxSet '{Name}'", TmdbCollectionKey, id, bs.Name);
                }

                done++;
                progress.Report(total == 0 ? 100 : done * 100.0 / total);
                await Task.Yield();
            }

            _logger.LogInformation("[TBMFC] ScanTask finished. BoxSets scanned: {Count}", total);
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => Array.Empty<TaskTriggerInfo>();

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

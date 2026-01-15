using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class ScanTask : IScheduledTask
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<ScanTask> _logger;

        public ScanTask(ILibraryManager libraryManager, ILogger<ScanTask> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

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

                var existing = bs.GetProviderId(MetadataProvider.TmdbCollection);
                if (!string.IsNullOrWhiteSpace(existing))
                {
                    done++;
                    progress.Report(total == 0 ? 100 : (done * 100.0 / total));
                    continue;
                }

                var movies = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    ParentId = bs.Id,
                    Recursive = true,
                    IncludeItemTypes = new[] { BaseItemKind.Movie }
                }).OfType<Movie>();

                var id = movies.Select(m => m.GetProviderId(MetadataProvider.TmdbCollection))
                               .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                if (!string.IsNullOrWhiteSpace(id))
                {
                    bs.SetProviderId(MetadataProvider.TmdbCollection, id);
                    _logger.LogInformation("[TBMFC] Set TMDbCollectionId={Id} on BoxSet '{Name}'", id, bs.Name);

                    // Optional: Nutzer kann danach in Jellyfin "Bilder ersetzen" nutzen.
                    // Wir triggern hier bewusst keinen aggressiven Provider-Refresh,
                    // um unnötige TMDb Calls zu vermeiden.
                }

                done++;
                progress.Report(total == 0 ? 100 : (done * 100.0 / total));

                // kleine Yield, damit UI nicht hängt
                await Task.Yield();
            }

            _logger.LogInformation("[TBMFC] ScanTask finished. BoxSets scanned: {Count}", total);
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
            => Array.Empty<TaskTriggerInfo>();
    }
}

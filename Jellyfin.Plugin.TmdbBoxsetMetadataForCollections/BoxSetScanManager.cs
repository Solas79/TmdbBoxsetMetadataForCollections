using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class BoxSetScanManager
    {
        private const string ProviderKeyTmdbCollection = "TmdbCollection";

        private readonly ILibraryManager libraryManager;
        private readonly IProviderManager providerManager;
        private readonly ILogger<BoxSetScanManager> logger;

        public BoxSetScanManager(
            ILibraryManager libraryManager,
            IProviderManager providerManager,
            ILogger<BoxSetScanManager> logger)
        {
            this.libraryManager = libraryManager;
            this.providerManager = providerManager;
            this.logger = logger;
        }

        public async Task RunAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            progress.Report(0);

            // NOTE: In 10.11.x we can query by string item types reliably.
            var boxSets = this.libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "BoxSet" },
                Recursive = true
            }).OfType<BoxSet>().ToArray();

            this.logger.LogInformation("[TBMFC] Found {Count} collections (BoxSet).", boxSets.Length);

            var processed = 0;
            var updated = 0;

            foreach (var boxSet in boxSets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                processed++;
                progress.Report(processed / (double)Math.Max(1, boxSets.Length));

                try
                {
                    // If already set, skip.
                    if (TryGetProviderId(boxSet, ProviderKeyTmdbCollection, out var existing) &&
                        !string.IsNullOrWhiteSpace(existing))
                    {
                        continue;
                    }

                    // Load movies in this collection.
                    var movies = this.libraryManager.GetItemList(new InternalItemsQuery
                    {
                        ParentId = boxSet.Id,
                        IncludeItemTypes = new[] { "Movie" },
                        Recursive = true
                    }).OfType<Movie>().ToArray();

                    if (movies.Length == 0)
                    {
                        continue;
                    }

                    // Derive first TMDbCollection id from movies.
                    string tmdbCollectionId = null;
                    foreach (var m in movies)
                    {
                        if (TryGetProviderId(m, ProviderKeyTmdbCollection, out var v) &&
                            !string.IsNullOrWhiteSpace(v))
                        {
                            tmdbCollectionId = v;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(tmdbCollectionId))
                    {
                        continue;
                    }

                    EnsureProviderIds(boxSet);
                    boxSet.ProviderIds[ProviderKeyTmdbCollection] = tmdbCollectionId;

                    this.logger.LogInformation(
                        "[TBMFC] Set {Key}={Val} on collection '{Name}' ({Id}).",
                        ProviderKeyTmdbCollection, tmdbCollectionId, boxSet.Name, boxSet.Id);

                    // Refresh metadata to trigger image fetch + write provider ids.
                    // This is the most compatible route across 10.11 builds.
                    await this.providerManager.RefreshMetadata(
                            boxSet,
                            new MetadataRefreshOptions
                            {
                                ForceSave = true,
                                ForceRefresh = true
                            },
                            cancellationToken)
                        .ConfigureAwait(false);

                    updated++;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "[TBMFC] Error processing collection '{Name}' ({Id}).", boxSet.Name, boxSet.Id);
                }
            }

            progress.Report(1);
            this.logger.LogInformation("[TBMFC] Scan finished. Updated {Updated} collections.", updated);
        }

        private static void EnsureProviderIds(BaseItem item)
        {
            if (item.ProviderIds == null)
            {
                item.ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static bool TryGetProviderId(BaseItem item, string key, out string value)
        {
            value = null;

            if (item.ProviderIds == null)
            {
                return false;
            }

            return item.ProviderIds.TryGetValue(key, out value);
        }
    }

    // Minimal options object used by IProviderManager.RefreshMetadata in 10.11.
    // If your Jellyfin.Controller already contains MediaBrowser.Controller.Providers.MetadataRefreshOptions,
    // this local type must be REMOVED. Only keep it if build complains it can't find MetadataRefreshOptions.
    public sealed class MetadataRefreshOptions
    {
        public bool ForceRefresh { get; set; }

        public bool ForceSave { get; set; }
    }
}

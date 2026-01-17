using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class BoxSetScanManager
    {
        private const string ProviderKeyTmdbCollection = "TmdbCollection";

        private readonly ILibraryManager libraryManager;
        private readonly ILogger<BoxSetScanManager> logger;

        public BoxSetScanManager(ILibraryManager libraryManager, ILogger<BoxSetScanManager> logger)
        {
            this.libraryManager = libraryManager;
            this.logger = logger;
        }

        public async Task RunAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            progress.Report(0);

            // 1) Alle BoxSets/Collections holen
            var boxSets = this.libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.BoxSet },
                Recursive = true
            }).OfType<BoxSet>().ToArray();

            this.logger.LogInformation("Found {Count} collections (BoxSets).", boxSets.Length);

            var processed = 0;

            foreach (var boxSet in boxSets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    processed++;
                    progress.Report(processed / (double)Math.Max(1, boxSets.Length));

                    // Hat die Collection schon eine TMDbCollection?
                    string existing;
                    if (boxSet.ProviderIds != null
                        && boxSet.ProviderIds.TryGetValue(ProviderKeyTmdbCollection, out existing)
                        && !string.IsNullOrWhiteSpace(existing))
                    {
                        continue;
                    }

                    // 2) Filme in der Collection holen
                    var children = this.libraryManager.GetItemList(new InternalItemsQuery
                    {
                        ParentId = boxSet.Id,
                        IncludeItemTypes = new[] { BaseItemKind.Movie },
                        Recursive = true
                    }).OfType<Movie>().ToArray();

                    if (children.Length == 0)
                    {
                        continue;
                    }

                    // 3) Erste gültige TmdbCollection aus Filmen nehmen
                    var tmdbCollectionId = children
                        .Select(m =>
                        {
                            string v;
                            return (m.ProviderIds != null && m.ProviderIds.TryGetValue(ProviderKeyTmdbCollection, out v)) ? v : null;
                        })
                        .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

                    if (string.IsNullOrWhiteSpace(tmdbCollectionId))
                    {
                        // Kein Film hat eine TmdbCollection
                        continue;
                    }

                    this.logger.LogInformation("Set {Key}={Val} on collection '{Name}' ({Id}).",
                        ProviderKeyTmdbCollection, tmdbCollectionId, boxSet.Name, boxSet.Id);

                    // 4) Setzen + speichern
                    if (boxSet.ProviderIds == null)
                    {
                        boxSet.ProviderIds = new ProviderIdDictionary(StringComparer.OrdinalIgnoreCase);
                    }

                    boxSet.ProviderIds[ProviderKeyTmdbCollection] = tmdbCollectionId;

                    // 5) Persistieren
                    this.libraryManager.UpdateItem(boxSet, boxSet.GetParent(), ItemUpdateType.MetadataEdit);

                    // 6) Optional: Refresh Metadata (damit Bilder gezogen werden)
                    await boxSet.RefreshMetadata(new MetadataRefreshOptions(cancellationToken, MetadataRefreshMode.FullRefresh), cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error processing collection {Name} ({Id}).", boxSet.Name, boxSet.Id);
                }
            }

            progress.Report(1);
            this.logger.LogInformation("Scan finished.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.ScheduledTasks;

public sealed class ScanLibraryTask : IScheduledTask
{
    private readonly ILibraryManager libraryManager;
    private readonly IItemRepository itemRepository;
    private readonly ILogger<ScanLibraryTask> logger;

    public ScanLibraryTask(
        ILibraryManager libraryManager,
        IItemRepository itemRepository,
        ILogger<ScanLibraryTask> logger)
    {
        this.libraryManager = libraryManager;
        this.itemRepository = itemRepository;
        this.logger = logger;
    }

    public string Name => "TMDb Boxset Metadata for Collections: Apply IDs";

    public string Key => "TmdbBoxsetMetadataForCollectionsApplyIds";

    public string Description => "Sets ProviderIds['Tmdb'] on BoxSets using first contained Movie's ProviderIds['TmdbCollection'].";

    public string Category => "Library";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => Array.Empty<TaskTriggerInfo>();

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);
        logger.LogInformation("[TMDB-COLLECTION] Manual scan started.");

        var boxSets = libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.BoxSet },
            Recursive = true,
        }).ToList();

        logger.LogInformation("[TMDB-COLLECTION] Found {Count} BoxSets.", boxSets.Count);

        var updated = 0;
        var processed = 0;

        foreach (var boxSet in boxSets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            processed++;

            try
            {
                if (boxSet?.ProviderIds != null
                    && boxSet.ProviderIds.TryGetValue("Tmdb", out var existingTmdb)
                    && !string.IsNullOrWhiteSpace(existingTmdb))
                {
                    continue;
                }

                var movies = libraryManager.GetItemList(new InternalItemsQuery
                {
                    ParentId = boxSet.Id,
                    IncludeItemTypes = new[] { BaseItemKind.Movie },
                    Recursive = true,
                }).ToList();

                if (movies.Count == 0)
                {
                    continue;
                }

                string tmdbCollectionId = null;

                foreach (var movie in movies)
                {
                    if (movie?.ProviderIds == null)
                    {
                        continue;
                    }

                    if (movie.ProviderIds.TryGetValue("TmdbCollection", out var v) && !string.IsNullOrWhiteSpace(v))
                    {
                        tmdbCollectionId = v.Trim();
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(tmdbCollectionId))
                {
                    continue;
                }

                if (!long.TryParse(tmdbCollectionId, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    logger.LogWarning("[TMDB-COLLECTION] BoxSet '{Name}' got non-numeric TmdbCollection='{Id}'. Skipping.",
                        boxSet.Name, tmdbCollectionId);
                    continue;
                }

                if (boxSet.ProviderIds == null)
                {
                    boxSet.ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                boxSet.ProviderIds["Tmdb"] = tmdbCollectionId;

                // Persistieren in DB/FS (Jellyfin kümmert sich um collection.xml bei FileSystem-Items)
                itemRepository.SaveItem(boxSet, cancellationToken);

                updated++;
                logger.LogInformation("[TMDB-COLLECTION] Updated BoxSet '{Name}' => ProviderIds['Tmdb']={Id}",
                    boxSet.Name, tmdbCollectionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[TMDB-COLLECTION] Error processing BoxSet '{Name}' ({Id})", boxSet?.Name, boxSet?.Id);
            }

            progress.Report(boxSets.Count == 0 ? 100 : (processed * 100.0 / boxSets.Count));
            await Task.Yield();
        }

        logger.LogInformation("[TMDB-COLLECTION] Finished. Updated {Updated} BoxSets.", updated);
        progress.Report(100);
    }
}

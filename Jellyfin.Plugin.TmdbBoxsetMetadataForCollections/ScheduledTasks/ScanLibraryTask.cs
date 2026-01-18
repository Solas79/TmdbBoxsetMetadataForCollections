using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.ScheduledTasks;

public sealed class ScanLibraryTask : IScheduledTask
{
    private readonly ILibraryManager libraryManager;
    private readonly ILogger<ScanLibraryTask> logger;

    public ScanLibraryTask(ILibraryManager libraryManager, ILogger<ScanLibraryTask> logger)
    {
        this.libraryManager = libraryManager;
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

        // Reflection: finde eine Persist-Methode im ILibraryManager
        var persist = CreatePersistDelegate(libraryManager, logger);

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

                // Persistieren (dynamisch, weil API je nach Jellyfin-Version variiert)
                persist(boxSet, cancellationToken);

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

    private static Action<BaseItem, CancellationToken> CreatePersistDelegate(ILibraryManager libraryManager, ILogger logger)
    {
        // Wir versuchen in dieser Reihenfolge typische Persist-Methoden:
        // 1) UpdateItem(BaseItem, ItemUpdateType, CancellationToken)
        // 2) UpdateItems(IEnumerable<BaseItem>, ItemUpdateType, CancellationToken)
        // 3) ItemRepository-ähnliche Methoden gibt es hier nicht – nur LibraryManager-Update ist üblich.

        var lmType = libraryManager.GetType();
        logger.LogInformation("[TMDB-COLLECTION] ILibraryManager concrete type: {Type}", lmType.FullName);

        // 1) UpdateItem
        var m1 = lmType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m =>
                m.Name == "UpdateItem" &&
                m.GetParameters().Length == 3 &&
                typeof(BaseItem).IsAssignableFrom(m.GetParameters()[0].ParameterType) &&
                m.GetParameters()[1].ParameterType.Name == "ItemUpdateType" &&
                m.GetParameters()[2].ParameterType == typeof(CancellationToken));

        if (m1 != null)
        {
            logger.LogInformation("[TMDB-COLLECTION] Persist via ILibraryManager.UpdateItem(BaseItem, ItemUpdateType, CancellationToken)");
            return (item, ct) =>
            {
                var updateType = Enum.Parse(m1.GetParameters()[1].ParameterType, "MetadataEdit");
                m1.Invoke(libraryManager, new object[] { item, updateType, ct });
            };
        }

        // 2) UpdateItems
        var m2 = lmType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m =>
                m.Name == "UpdateItems" &&
                m.GetParameters().Length == 3 &&
                m.GetParameters()[1].ParameterType.Name == "ItemUpdateType" &&
                m.GetParameters()[2].ParameterType == typeof(CancellationToken));

        if (m2 != null)
        {
            logger.LogInformation("[TMDB-COLLECTION] Persist via ILibraryManager.UpdateItems(..., ItemUpdateType, CancellationToken)");
            return (item, ct) =>
            {
                var updateType = Enum.Parse(m2.GetParameters()[1].ParameterType, "MetadataEdit");
                // param0 ist irgendein IEnumerable<...> -> wir geben List<BaseItem>
                m2.Invoke(libraryManager, new object[] { new List<BaseItem> { item }, updateType, ct });
            };
        }

        // Wenn gar nichts gefunden wurde: wir brechen bewusst mit klarer Meldung ab.
        logger.LogError("[TMDB-COLLECTION] No compatible persist method found on ILibraryManager. Cannot save changes.");
        return (_, _) => throw new InvalidOperationException("No compatible persist method found on ILibraryManager.");
    }
}

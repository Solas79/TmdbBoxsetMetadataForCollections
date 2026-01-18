using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<ScanLibraryTask> _logger;

    public ScanLibraryTask(ILibraryManager libraryManager, ILogger<ScanLibraryTask> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public string Name => "TMDb Boxset Metadata for Collections: Apply IDs";

    public string Key => "TmdbBoxsetMetadataForCollectionsApplyIds";

    public string Description => "Finds collections (BoxSet) without ProviderIds['Tmdb'] and sets it from first movie's ProviderIds['TmdbCollection'].";

    public string Category => "Library";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // KEIN Auto-Trigger – nur manuell.
        return Array.Empty<TaskTriggerInfo>();
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);

        _logger.LogInformation("[TMDB-COLLECTION] Manual scan started.");

        // 1) Alle BoxSets holen
        var boxSets = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.BoxSet },
            Recursive = true
        }).OfType<BoxSet>().ToList();

        _logger.LogInformation("[TMDB-COLLECTION] Found {Count} BoxSets.", boxSets.Count);

        var updated = 0;
        var processed = 0;

        foreach (var boxSet in boxSets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            processed++;

            try
            {
                // Skip wenn Tmdb schon da ist
                var hasTmdb = boxSet.ProviderIds != null
                              && boxSet.ProviderIds.TryGetValue("Tmdb", out var existing)
                              && !string.IsNullOrWhiteSpace(existing);

                if (hasTmdb)
                {
                    continue;
                }

                // 2) Movies in BoxSet holen
                var movies = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    ParentId = boxSet.Id,
                    IncludeItemTypes = new[] { BaseItemKind.Movie },
                    Recursive = true
                }).OfType<Movie>().ToList();

                if (movies.Count == 0)
                {
                    continue;
                }

                // 3) Erste Movie mit TmdbCollection finden
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
                    // Kein Film in der Collection hat TmdbCollection
                    continue;
                }

                // Optional: harte Prüfung "nur Zahlen"
                if (!long.TryParse(tmdbCollectionId, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    _logger.LogWarning("[TMDB-COLLECTION] BoxSet '{Name}' got non-numeric TmdbCollection='{Id}' from a movie. Skipping.",
                        boxSet.Name, tmdbCollectionId);
                    continue;
                }

                // 4) In BoxSet schreiben: ProviderIds["Tmdb"] = TMDb Collection ID
                if (boxSet.ProviderIds == null)
                {
                    boxSet.ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                boxSet.ProviderIds["Tmdb"] = tmdbCollectionId;

                // 5) Speichern
                // UpdateToRepository existiert in Jellyfin 10.11.x Items (siehe Stacktraces in Jellyfin Issues).
                boxSet.UpdateToRepository(ItemUpdateType.MetadataEdit, cancellationToken);

                updated++;
                _logger.LogInformation("[TMDB-COLLECTION] Updated BoxSet '{Name}' => ProviderIds['Tmdb']={Id}",
                    boxSet.Name, tmdbCollectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TMDB-COLLECTION] Error processing BoxSet '{Name}' ({Id})", boxSet?.Name, boxSet?.Id);
            }

            progress.Report(boxSets.Count == 0 ? 100 : (processed * 100.0 / boxSets.Count));

            // minimal async yield
            await Task.Yield();
        }

        _logger.LogInformation("[TMDB-COLLECTION] Finished. Updated {Updated} BoxSets.", updated);
        progress.Report(100);
    }
}

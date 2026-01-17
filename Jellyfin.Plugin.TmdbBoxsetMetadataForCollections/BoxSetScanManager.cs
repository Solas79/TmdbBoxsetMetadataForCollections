// <copyright file="BoxSetScanManager.cs" company="Jellyfin">
// Copyright (c) Jellyfin.
// Licensed under the GNU General Public License v2.0.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

/// <summary>
/// Scans existing collections and assigns TMDb collection ids based on contained movies.
/// </summary>
public sealed class BoxSetScanManager
{
    private const string ProviderKeyTmdbCollection = "TmdbCollection";

    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<BoxSetScanManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoxSetScanManager"/> class.
    /// </summary>
    public BoxSetScanManager(ILibraryManager libraryManager, ILogger<BoxSetScanManager> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    /// <summary>
    /// Execute scan.
    /// </summary>
    public async Task RunAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);

        var boxSets = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.BoxSet },
            Recursive = true,
        }).OfType<BoxSet>().ToList();

        _logger.LogInformation("[TBMFC] Found {Count} collections (BoxSet).", boxSets.Count);

        var processed = 0;
        foreach (var boxSet in boxSets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            processed++;

            try
            {
                var currentId = GetProviderId(boxSet, ProviderKeyTmdbCollection);
                if (!string.IsNullOrWhiteSpace(currentId))
                {
                    continue;
                }

                var movies = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    ParentId = boxSet.Id,
                    IncludeItemTypes = new[] { BaseItemKind.Movie },
                    Recursive = true,
                }).OfType<Movie>().ToList();

                if (movies.Count == 0)
                {
                    continue;
                }

                var derived = movies
                    .Select(m => GetProviderId(m, ProviderKeyTmdbCollection))
                    .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

                if (string.IsNullOrWhiteSpace(derived))
                {
                    continue;
                }

                SetProviderId(boxSet, ProviderKeyTmdbCollection, derived);

                _logger.LogInformation(
                    "[TBMFC] Set {Key}={Value} on collection '{Name}' ({Id}).",
                    ProviderKeyTmdbCollection,
                    derived,
                    boxSet.Name,
                    boxSet.Id);

                // Trigger metadata refresh so artwork/metadata can be pulled
                await boxSet.RefreshMetadata(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TBMFC] Error processing collection '{Name}' ({Id}).", boxSet.Name, boxSet.Id);
            }

            progress.Report(processed * 100d / Math.Max(1, boxSets.Count));
        }

        progress.Report(100);
        _logger.LogInformation("[TBMFC] Scan finished.");
    }

    private static string GetProviderId(BaseItem item, string key)
    {
        if (item.ProviderIds is null)
        {
            return string.Empty;
        }

        return item.ProviderIds.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static void SetProviderId(BaseItem item, string key, string value)
    {
        item.ProviderIds ??= new ProviderIdDictionary();
        item.ProviderIds[key] = value;
    }
}

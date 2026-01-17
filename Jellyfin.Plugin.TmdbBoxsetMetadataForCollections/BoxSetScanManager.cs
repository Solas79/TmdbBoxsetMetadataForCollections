// <copyright file="BoxSetScanManager.cs" company="Jellyfin">
// Copyright (c) Jellyfin.
// Licensed under the GNU General Public License v2.0.
// </copyright>

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Jellyfin.Data.Enums;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Entities.Movies;
    using MediaBrowser.Controller.Library;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Scans existing collections and assigns TMDb collection ids based on contained movies.
    /// </summary>
    public sealed class BoxSetScanManager
    {
        private const string ProviderKeyTmdbCollection = "TmdbCollection";

        private readonly ILibraryManager libraryManager;
        private readonly ILogger<BoxSetScanManager> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxSetScanManager"/> class.
        /// </summary>
        /// <param name="libraryManager">Library manager.</param>
        /// <param name="logger">Logger.</param>
        public BoxSetScanManager(ILibraryManager libraryManager, ILogger<BoxSetScanManager> logger)
        {
            this.libraryManager = libraryManager;
            this.logger = logger;
        }

        /// <summary>
        /// Execute scan.
        /// </summary>
        /// <param name="progress">Progress reporter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task RunAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            progress.Report(0);

            var boxSets = this.libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.BoxSet },
                Recursive = true,
            }).OfType<BoxSet>().ToList();

            this.logger.LogInformation("[TBMFC] Found {Count} collections (BoxSet).", boxSets.Count);

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

                    var movies = this.libraryManager.GetItemList(new InternalItemsQuery
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

                    this.logger.LogInformation(
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
                    this.logger.LogError(ex, "[TBMFC] Error processing collection '{Name}' ({Id}).", boxSet.Name, boxSet.Id);
                }

                progress.Report(processed * 100d / Math.Max(1, boxSets.Count));
            }

            progress.Report(100);
            this.logger.LogInformation("[TBMFC] Scan finished.");
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
            // In 10.11.x ist ProviderIds Dictionary-kompatibel; falls null setzen wir ein normales Dictionary.
            if (item.ProviderIds is null)
            {
                item.ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            item.ProviderIds[key] = value;
        }
    }
}

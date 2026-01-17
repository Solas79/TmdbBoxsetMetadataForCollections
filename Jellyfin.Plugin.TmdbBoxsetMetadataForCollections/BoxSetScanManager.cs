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
    /// Scans collections (BoxSet) and assigns TMDbCollection provider id based on contained movies.
    /// </summary>
    public sealed class BoxSetScanManager
    {
        private const string ProviderKeyTmdbCollection = "TmdbCollection";

        private static readonly Action<ILogger, int, Exception> LogFoundCollections =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1, nameof(LogFoundCollections)),
                "[TBMFC] Found {Count} collections (BoxSet).");

        private static readonly Action<ILogger, string, string, string, Guid, Exception> LogSetId =
            LoggerMessage.Define<string, string, string, Guid>(
                LogLevel.Information,
                new EventId(2, nameof(LogSetId)),
                "[TBMFC] Set {Key}={Value} on collection '{Name}' ({Id}).");

        private static readonly Action<ILogger, int, Exception> LogFinished =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(3, nameof(LogFinished)),
                "[TBMFC] Scan finished. Updated {Changed} collections.");

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
            ArgumentNullException.ThrowIfNull(progress);

            progress.Report(0);

            var boxSets = this.libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.BoxSet },
                Recursive = true,
            }).OfType<BoxSet>().ToList();

            LogFoundCollections(this.logger, boxSets.Count, null);

            var processed = 0;
            var changed = 0;

            foreach (var boxSet in boxSets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                processed++;

                var currentId = GetProviderId(boxSet, ProviderKeyTmdbCollection);
                if (!string.IsNullOrWhiteSpace(currentId))
                {
                    progress.Report(processed * 100d / Math.Max(1, boxSets.Count));
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
                    progress.Report(processed * 100d / Math.Max(1, boxSets.Count));
                    continue;
                }

                var derived = movies
                    .Select(m => GetProviderId(m, ProviderKeyTmdbCollection))
                    .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

                if (string.IsNullOrWhiteSpace(derived))
                {
                    progress.Report(processed * 100d / Math.Max(1, boxSets.Count));
                    continue;
                }

                SetProviderId(boxSet, ProviderKeyTmdbCollection, derived);
                changed++;

                LogSetId(this.logger, ProviderKeyTmdbCollection, derived, boxSet.Name, boxSet.Id, null);

                // Refresh metadata so artwork/metadata can be pulled.
                await boxSet.RefreshMetadata(cancellationToken).ConfigureAwait(false);

                progress.Report(processed * 100d / Math.Max(1, boxSets.Count));
            }

            progress.Report(100);
            LogFinished(this.logger, changed, null);
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
            if (item.ProviderIds is null)
            {
                item.ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            item.ProviderIds[key] = value;
        }
    }
}

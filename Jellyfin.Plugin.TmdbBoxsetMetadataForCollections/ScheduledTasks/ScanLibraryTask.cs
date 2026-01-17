// <copyright file="ScanLibraryTask.cs" company="Jellyfin">
// Copyright (c) Jellyfin.
// Licensed under the GNU General Public License v2.0.
// </copyright>

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.ScheduledTasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediaBrowser.Model.Tasks;

    /// <summary>
    /// Manual scheduled task to run the scan.
    /// </summary>
    public sealed class ScanLibraryTask : IScheduledTask
    {
        /// <inheritdoc />
        public string Name => "TMDb Boxset Metadata for Collections";

        /// <inheritdoc />
        public string Description =>
            "Manual scan: assign TMDb boxset/collection ids to collections based on contained movies.";

        /// <inheritdoc />
        public string Category => "Library";

        /// <inheritdoc />
        public string Key => "tmdb_boxset_metadata_for_collections_scan";

        /// <inheritdoc />
        public bool IsHidden => false;

        /// <inheritdoc />
        public bool IsEnabled => true;

        /// <inheritdoc />
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // Manual only.
            yield break;
        }

        /// <inheritdoc />
        public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            progress.Report(0);

            // Scan logic comes next.

            progress.Report(100);
            return Task.CompletedTask;
        }
    }
}

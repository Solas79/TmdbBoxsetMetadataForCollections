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
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Manual scheduled task to run the scan.
    /// </summary>
    public sealed class ScanLibraryTask : IScheduledTask
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanLibraryTask"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public ScanLibraryTask(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public string Name => "TMDb Boxset Metadata for Collections";

        /// <inheritdoc />
        public string Description =>
            "Manual scan: copies ProviderIds['TmdbCollection'] from movies into their collections and refreshes metadata.";

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
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            var mgr = this.serviceProvider.GetRequiredService<BoxSetScanManager>();
            await mgr.RunAsync(progress, cancellationToken).ConfigureAwait(false);
        }
    }
}

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
        private readonly bool isHidden;
        private readonly bool isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanLibraryTask"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public ScanLibraryTask(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.isHidden = false;
            this.isEnabled = true;
        }

        /// <summary>
        /// Gets the task name.
        /// </summary>
        public string Name => "TMDb Boxset Metadata for Collections";

        /// <summary>
        /// Gets the task description.
        /// </summary>
        public string Description =>
            "Manual scan: copies ProviderIds['TmdbCollection'] from movies into their collections and refreshes metadata.";

        /// <summary>
        /// Gets the task category.
        /// </summary>
        public string Category => "Library";

        /// <summary>
        /// Gets the task key.
        /// </summary>
        public string Key => "tmdb_boxset_metadata_for_collections_scan";

        /// <summary>
        /// Gets a value indicating whether the task is hidden.
        /// </summary>
        public bool IsHidden => this.isHidden;

        /// <summary>
        /// Gets a value indicating whether the task is enabled.
        /// </summary>
        public bool IsEnabled => this.isEnabled;

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

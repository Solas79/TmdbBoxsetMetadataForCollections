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
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Model.Tasks;
    using Microsoft.Extensions.Logging;

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

        public string Name => "TMDb Boxset Metadata for Collections";

        public string Description =>
            "Manual scan: copies ProviderIds['TmdbCollection'] from movies into their collections and refreshes metadata.";

        public string Category => "Library";

        public string Key => "tmdb_boxset_metadata_for_collections_scan";

        public bool IsHidden => this.isHidden;

        public bool IsEnabled => this.isEnabled;

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // Manual only.
            yield break;
        }

        public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            var libraryManager = (ILibraryManager)this.serviceProvider.GetService(typeof(ILibraryManager));
            var loggerFactory = (ILoggerFactory)this.serviceProvider.GetService(typeof(ILoggerFactory));

            if (libraryManager is null || loggerFactory is null)
            {
                throw new InvalidOperationException("Required Jellyfin services were not available (ILibraryManager/ILoggerFactory).");
            }

            var logger = loggerFactory.CreateLogger<BoxSetScanManager>();
            var mgr = new BoxSetScanManager(libraryManager, logger);

            return mgr.RunAsync(progress, cancellationToken);
        }
    }
}

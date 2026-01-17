// <copyright file="ScanLibraryTask.cs" company="Jellyfin">
// Copyright (c) Jellyfin.
// Licensed under the GNU General Public License v2.0.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.ScheduledTasks;

/// <summary>
/// Manual scheduled task to run the scan.
/// </summary>
public sealed class ScanLibraryTask : IScheduledTask
{
    private readonly BoxSetScanManager _scanManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryTask"/> class.
    /// </summary>
    public ScanLibraryTask(BoxSetScanManager scanManager)
    {
        _scanManager = scanManager;
    }

    /// <inheritdoc />
    public string Name => "Scan collections for TMDb Boxset IDs";

    /// <inheritdoc />
    public string Description =>
        "Assigns TMDbCollection ids to existing collections based on their contained movies and refreshes metadata.";

    /// <inheritdoc />
    public string Category => "TMDb Boxset Metadata for Collections";

    /// <inheritdoc />
    public string Key => "tmdb_boxset_metadata_for_collections_scan";

    /// <inheritdoc />
    public bool IsHidden => false;

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // Manual only => no triggers
        return Array.Empty<TaskTriggerInfo>();
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        await _scanManager.RunAsync(progress, cancellationToken).ConfigureAwait(false);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.ScheduledTasks
{
    public sealed class ScanLibraryTask : IScheduledTask
    {
        private readonly BoxSetScanManager scanManager;

        public ScanLibraryTask(BoxSetScanManager scanManager)
        {
            this.scanManager = scanManager;
        }

        public string Name => "TMDb Boxset Metadata: Scan Collections";

        public string Key => "TmdbBoxsetMetadataForCollectionsScan";

        public string Description => "Assign TMDbCollection id to collections based on contained movies, then refresh metadata.";

        public string Category => "Library";

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // KEIN Auto-Trigger -> nur manuell
            return Array.Empty<TaskTriggerInfo>();
        }

        public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            return this.scanManager.RunAsync(progress, cancellationToken);
        }
    }
}

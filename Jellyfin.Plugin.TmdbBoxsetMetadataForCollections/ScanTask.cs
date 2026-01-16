using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class ScanTask : IScheduledTask
    {
        private readonly BoxSetScanManager _manager;

        public ScanTask(BoxSetScanManager manager)
        {
            _manager = manager;
        }

        public string Key => "TBMFC.ScanTask";
        public string Name => "Scan BoxSets: copy TMDbCollection IDs from movies";
        public string Description => "Sets TMDbCollection on BoxSets and can trigger metadata/image refresh.";
        public string Category => "TMDb Boxset Metadata for Collections";

        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Hier mit Refresh=true, damit Bilder kommen:
            await _manager.ScanAsync(triggerRefresh: true, progress: progress, ct: cancellationToken);
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => Array.Empty<TaskTriggerInfo>();
    }
}

using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class EntryPoint : IServerEntryPoint
    {
        private readonly BoxSetScanManager _manager;
        private readonly ILogger<EntryPoint> _logger;

        public EntryPoint(BoxSetScanManager manager, ILogger<EntryPoint> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        public Task RunAsync()
        {
            // Fire-and-forget beim Start: kein aggressiver Refresh, nur IDs setzen.
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("[TBMFC] EntryPoint started");
                    await _manager.ScanAsync(triggerRefresh: false, progress: null, ct: CancellationToken.None);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "[TBMFC] EntryPoint scan failed");
                }
            });

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    /// <summary>
    /// Läuft beim Serverstart und führt genau einmal einen Scan aus.
    /// </summary>
    public sealed class StartupHostedService : IHostedService
    {
        private readonly ILogger<StartupHostedService> _logger;
        private readonly BoxSetScanManager _scanManager;

        public StartupHostedService(ILogger<StartupHostedService> logger, BoxSetScanManager scanManager)
        {
            _logger = logger;
            _scanManager = scanManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // kleiner Delay, damit Jellyfin/Library nach Startup stabil ist
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                    _logger.LogInformation("[TBMFC] StartupHostedService: Scan startet...");
                    await _scanManager.ScanAsync(triggerRefresh: true, progress: null, ct: cancellationToken);
                    _logger.LogInformation("[TBMFC] StartupHostedService: Scan fertig.");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("[TBMFC] StartupHostedService: abgebrochen.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[TBMFC] StartupHostedService: Fehler beim Scan.");
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

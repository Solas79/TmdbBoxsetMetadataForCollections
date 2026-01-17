using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

public sealed class StartupHostedService : IHostedService
{
    private readonly BoxSetScanService _scanner;
    private readonly ILogger<StartupHostedService> _logger;

    public StartupHostedService(
        BoxSetScanService scanner,
        ILogger<StartupHostedService> logger)
    {
        _scanner = scanner;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[TBMFC] Startup scan starting");
        await _scanner.RunAsync(cancellationToken);
        _logger.LogInformation("[TBMFC] Startup scan finished");
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

public sealed class BoxSetScanService
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<BoxSetScanService> _logger;

    public BoxSetScanService(
        ILibraryManager libraryManager,
        ILogger<BoxSetScanService> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[TBMFC] Scan started (placeholder)");
        // Hier kommt später die TMDb-Logik rein
        return Task.CompletedTask;
    }
}

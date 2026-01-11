using MediaBrowser.Common.ScheduledTasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TmdbBoxsetMetadataForCollections.Services;

namespace TmdbBoxsetMetadataForCollections.Tasks;

public class ApplyBoxsetMetadataTask : IScheduledTask
{
    private readonly CollectionScanner _scanner;
    private readonly ILogger<ApplyBoxsetMetadataTask> _logger;

    public ApplyBoxsetMetadataTask(CollectionScanner scanner, ILogger<ApplyBoxsetMetadataTask> logger)
    {
        _scanner = scanner;
        _logger = logger;
    }

    public string Name => "Apply TMDb Boxset metadata to Collections";
    public string Key => "TmdbBoxsetMetadataForCollections";
    public string Description =>
        "Reads TMDb collection IDs from movies and applies the corresponding box set metadata to existing collections.";

    public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
    {
        _logger.LogInformation("[TMDb-Collections] Scan started");
        await _scanner.RunAsync(cancellationToken);
        _logger.LogInformation("[TMDb-Collections] Scan finished");
    }
}

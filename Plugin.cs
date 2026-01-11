using Jellyfin.Plugin;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.Logging;

namespace TmdbBoxsetMetadataForCollections;

public class Plugin : BasePlugin
{
    public override string Name => "TMDb Boxset Metadata for Collections";
    public override string Description =>
        "Applies TMDb box set metadata to existing Jellyfin collections based on movie TmdbCollection IDs.";

    public Plugin(IApplicationPaths paths, ILoggerFactory loggerFactory)
        : base(paths, loggerFactory)
    {
    }
}

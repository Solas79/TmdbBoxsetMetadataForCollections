using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

public class Plugin : BasePlugin
{
    public Plugin(IApplicationPaths paths, IXmlSerializer xml, ILogger<Plugin> logger)
        : base(paths, xml)
    {
        Instance = this;
        Logger = logger;
    }

    public static Plugin? Instance { get; private set; }
    public static ILogger<Plugin>? Logger { get; private set; }

    public override string Name => "TMDb Boxset Metadata For Collections";
    public override Guid Id => Guid.Parse("a9a6db5c-9fd3-44c0-b36a-8a26a9e4b1b5");
}

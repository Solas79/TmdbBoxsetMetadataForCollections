using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

public sealed class Plugin : BasePlugin
{
    public static Plugin Instance { get; private set; } = null!;

    public Plugin(IApplicationPaths applicationPaths)
        : base(applicationPaths)
    {
        Instance = this;
    }

    public override string Name => "TMDb Boxset Metadata for Collections";

    public override Guid Id => Guid.Parse("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

    public override string Description =>
        "Assigns TMDb boxset metadata to existing Jellyfin collections based on their movies.";
}

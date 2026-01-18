using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

public sealed class Plugin : BasePlugin
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin Instance { get; private set; }

    public override string Name => "TMDb Boxset Metadata for Collections";

    public override Guid Id => new Guid("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

    public override string Description =>
        "Adds TMDb Box Set ID to existing Jellyfin collections by reading TmdbCollection from contained movies. Manual task only.";
}

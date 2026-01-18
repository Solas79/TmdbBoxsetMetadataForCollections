using System;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

public sealed class Plugin : BasePlugin
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
    }

    public override string Name => "TMDb Boxset Metadata for Collections";

    public override Guid Id => new Guid("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

    public override string Description =>
        "Manual task: copies Movie.ProviderIds['TmdbCollection'] into BoxSet.ProviderIds['Tmdb'] so metadata/artwork can be fetched.";
}

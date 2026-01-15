using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class Plugin : BasePlugin<Configuration.PluginConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
        }

        public override string Name => "TMDb Boxset Metadata for Collections";

        public override Guid Id => Guid.Parse("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        public override string Description =>
            "Assigns TMDb collection (boxset) IDs to Jellyfin BoxSets based on the contained movies, enabling boxset metadata/images.";
    }
}

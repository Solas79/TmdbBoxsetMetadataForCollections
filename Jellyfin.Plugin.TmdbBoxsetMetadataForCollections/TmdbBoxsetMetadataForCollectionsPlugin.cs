using MediaBrowser.Common.Plugins;
using System;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class TmdbBoxsetMetadataForCollectionsPlugin : BasePlugin
    {
        public static TmdbBoxsetMetadataForCollectionsPlugin Instance { get; private set; }

        public TmdbBoxsetMetadataForCollectionsPlugin()
        {
            Instance = this;
        }

        public override string Name => "TMDb Boxset Metadata for Collections";

        public override Guid Id => new Guid("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        public override string Description =>
            "Assigns TMDbCollection id to existing Jellyfin collections (BoxSets) based on contained movies.";
    }
}

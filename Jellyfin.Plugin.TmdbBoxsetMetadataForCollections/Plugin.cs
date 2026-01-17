using System;
using MediaBrowser.Common.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class Plugin : BasePlugin
    {
        public override string Name => "TMDb Boxset Metadata for Collections";

        public override Guid Id => Guid.Parse("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        public override string Description =>
            "Copies TMDbCollection IDs from movies into BoxSets (Collections) and triggers metadata/image refresh.";
    }
}

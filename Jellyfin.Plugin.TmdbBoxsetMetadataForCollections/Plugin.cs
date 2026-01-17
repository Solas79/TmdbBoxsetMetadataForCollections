using MediaBrowser.Common.Plugins;
using System;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class Plugin : BasePlugin
    {
        public Plugin()
        {
        }

        public override string Name => "TMDb Boxset Metadata for Collections";

        public override Guid Id =>
            new Guid("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");
    }
}

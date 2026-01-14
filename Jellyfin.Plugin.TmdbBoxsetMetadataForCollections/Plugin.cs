using System;
using MediaBrowser.Common.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class Plugin : IPlugin
    {
        public string Name => "TMDb Boxset Metadata for Collections";

        public Guid Id => Guid.Parse("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        public string Description =>
            "Automatically assigns TMDb boxset metadata to existing Jellyfin collections based on their movies.";

        public void Dispose()
        {
            // nichts zu entsorgen
        }
    }
}

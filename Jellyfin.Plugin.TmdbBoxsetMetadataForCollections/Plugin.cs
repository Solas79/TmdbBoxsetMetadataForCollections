using MediaBrowser.Common.Plugins;
using System;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public class Plugin : BasePlugin
    {
        public Plugin(IApplicationPaths applicationPaths)
            : base(applicationPaths)
        {
            Instance = this;
        }

        public static Plugin? Instance { get; private set; }

        public override string Name => "TMDb Boxset Metadata for Collections";

        public override Guid Id => new Guid("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        public override string Description =>
            "Automatically assigns TMDb boxset metadata to existing Jellyfin collections based on their movies.";
    }
}

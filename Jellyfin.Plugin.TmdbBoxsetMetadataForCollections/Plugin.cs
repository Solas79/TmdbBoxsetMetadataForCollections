using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class Plugin : BasePlugin
    {
        public static Plugin? Instance { get; private set; }

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name => "TMDb Boxset Metadata for Collections";

        public override Guid Id => Guid.Parse("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        public override string Description =>
            "Automatically assigns TMDb boxset metadata to existing Jellyfin collections based on their movies.";
    }
}

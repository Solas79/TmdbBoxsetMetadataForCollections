using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class TmdbBoxsetMetadataForCollectionsPlugin : BasePlugin
    {
        public static TmdbBoxsetMetadataForCollectionsPlugin Instance { get; private set; }

        public TmdbBoxsetMetadataForCollectionsPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name => "TMDb Boxset Metadata for Collections";

        public override Guid Id => new Guid("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        public override string Description =>
            "Copies TMDbCollection IDs from movies into BoxSets (collections) and can be run manually via scheduled task.";
    }
}

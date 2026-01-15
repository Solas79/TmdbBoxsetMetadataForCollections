using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.Configuration
{
    public sealed class PluginConfiguration : BasePluginConfiguration
    {
        public bool LogDebug { get; set; } = false;
    }
}

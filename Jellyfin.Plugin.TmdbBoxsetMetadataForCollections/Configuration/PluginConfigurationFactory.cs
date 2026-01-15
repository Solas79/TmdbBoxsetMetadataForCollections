using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.Configuration
{
    public sealed class PluginConfigurationFactory : IPluginConfigurationFactory
    {
        public BasePluginConfiguration CreateDefaultConfiguration() => new PluginConfiguration();
    }
}

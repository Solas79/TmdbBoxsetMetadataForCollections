using System;
using System.Reflection;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class Plugin : IPlugin
    {
        private static readonly Assembly ThisAssembly = typeof(Plugin).Assembly;

        public string Name => "TMDb Boxset Metadata for Collections";

        public Guid Id => Guid.Parse("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        public string Description =>
            "Automatically assigns TMDb boxset metadata to existing Jellyfin collections based on their movies.";

        public Version Version => ThisAssembly.GetName().Version ?? new Version(0, 0, 0, 0);

        public string AssemblyFilePath => ThisAssembly.Location;

        public string DataFolderPath => string.Empty;

        public bool CanUninstall => true;

        public PluginInfo GetPluginInfo()
        {
            // Signatur: PluginInfo(string name, Version version, string description, Guid id, bool canUninstall)
            return new PluginInfo(
                Name,
                Version,
                Description,
                Id,
                CanUninstall
            );
        }

        public void OnUninstalling()
        {
        }

        public void Dispose()
        {
        }
    }
}

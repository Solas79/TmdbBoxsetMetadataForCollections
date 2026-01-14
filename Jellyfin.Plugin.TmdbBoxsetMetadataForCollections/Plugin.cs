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

        // Jellyfin erwartet eine Version am Plugin-Objekt
        public Version Version => ThisAssembly.GetName().Version ?? new Version(0, 0, 0, 0);

        // Jellyfin nutzt das für Uninstall/Diagnose
        public string AssemblyFilePath => ThisAssembly.Location;

        // Datapath: wenn du keinen brauchst, leer lassen (nicht null!)
        public string DataFolderPath => string.Empty;

        // Ohne spezielle Uninstall-Logik: false (verhindert, dass Jellyfin in BasePlugin-NRE läuft)
        public bool CanUninstall => true;

        // Plugin-Info fürs UI
        public PluginInfo GetPluginInfo()
        {
            return new PluginInfo
            {
                Name = Name,
                Version = Version.ToString(),
                Description = Description,
                Id = Id.ToString()
            };
        }

        // Wird beim Deinstallieren aufgerufen – wir brauchen nichts
        public void OnUninstalling()
        {
        }

        public void Dispose()
        {
        }
    }
}

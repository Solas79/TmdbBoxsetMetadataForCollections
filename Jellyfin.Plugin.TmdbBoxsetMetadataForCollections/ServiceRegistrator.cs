using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    /// <summary>
    /// Registriert die Services des Plugins im Jellyfin-DI-Container.
    /// </summary>
    public sealed class ServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Zentrale Logik
            serviceCollection.AddSingleton<BoxSetScanManager>();

            // Geplante Aufgabe (erscheint in Jellyfin unter "Aufgaben")
            serviceCollection.AddSingleton<IScheduledTask, ScanTask>();

            // Einmaliger Scan beim Serverstart
            serviceCollection.AddSingleton<IHostedService, StartupHostedService>();
        }
    }
}

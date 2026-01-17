using Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.ScheduledTasks;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class ServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<BoxSetScanManager>();
            serviceCollection.AddTransient<ScanLibraryTask>();
        }
    }
}

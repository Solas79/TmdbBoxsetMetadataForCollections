using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Jellyfin.Server;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class ServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection services, IServerApplicationHost applicationHost)
        {
            // Metadata Provider registrieren
            services.AddTransient<TmdbCollectionMetadataProvider>();
        }
    }
}

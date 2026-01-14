using Microsoft.Extensions.DependencyInjection;
using MediaBrowser.Controller.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    public sealed class ServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection services)
        {
            // registriert unseren Metadata-Provider
            services.AddTransient<TmdbCollectionMetadataProvider>();
        }
    }
}

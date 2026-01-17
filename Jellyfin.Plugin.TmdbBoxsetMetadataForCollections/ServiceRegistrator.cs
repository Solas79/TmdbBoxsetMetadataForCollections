using Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;
using Microsoft.Extensions.DependencyInjection;
using MediaBrowser.Common.Plugins;

public sealed class ServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddHostedService<StartupHostedService>();
        services.AddSingleton<BoxSetScanService>();
    }
}

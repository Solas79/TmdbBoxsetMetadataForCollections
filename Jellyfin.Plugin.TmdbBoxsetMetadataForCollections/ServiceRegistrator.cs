// <copyright file="ServiceRegistrator.cs" company="Jellyfin">
// Copyright (c) Jellyfin.
// Licensed under the GNU General Public License v2.0.
// </copyright>

using Jellyfin.Plugin.TmdbBoxsetMetadataForCollections.ScheduledTasks;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

/// <summary>
/// Registers plugin services.
/// </summary>
public sealed class ServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<BoxSetScanManager>();
        serviceCollection.AddSingleton<IScheduledTask, ScanLibraryTask>();
    }
}

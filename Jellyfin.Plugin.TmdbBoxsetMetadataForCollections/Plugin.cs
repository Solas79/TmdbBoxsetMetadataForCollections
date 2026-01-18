using System;
using MediaBrowser.Common.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

/// <summary>
/// Plugin entry point.
/// </summary>
public sealed class Plugin : BasePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    public Plugin()
    {
    }

    /// <inheritdoc />
    public override string Name => "TMDb Boxset Metadata for Collections";

    /// <inheritdoc />
    public override Guid Id => new Guid("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

    /// <inheritdoc />
    public override string Description =>
        "Manual task: sets ProviderIds['Tmdb'] on BoxSets based on movies' ProviderIds['TmdbCollection'].";
}

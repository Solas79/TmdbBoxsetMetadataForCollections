// <copyright file="Plugin.cs" company="Jellyfin">
// Copyright (c) Jellyfin.
// Licensed under the GNU General Public License v2.0.
// </copyright>

using System;
using MediaBrowser.Common.Plugins;

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections;

/// <summary>
/// Plugin entry point.
/// </summary>
public sealed class Plugin : BasePlugin
{
    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public static Plugin Instance { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    public Plugin()
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "TMDb Boxset Metadata for Collections";

    /// <inheritdoc />
    public override Guid Id => new("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

    /// <inheritdoc />
    public override string Description =>
        "Assigns TMDb boxset/collection ids to existing Jellyfin collections based on their movies. Manual scan only.";
}

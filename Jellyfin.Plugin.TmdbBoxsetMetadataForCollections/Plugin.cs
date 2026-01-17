// <copyright file="Plugin.cs" company="Jellyfin">
// Copyright (c) Jellyfin.
// Licensed under the GNU General Public License v2.0.
// </copyright>

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    using System;
    using MediaBrowser.Common.Plugins;

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
            "Assigns TMDb boxset metadata to existing Jellyfin collections based on their movies. Manual task only.";
    }
}

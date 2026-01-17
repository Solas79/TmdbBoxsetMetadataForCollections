// <copyright file="Plugin.cs" company="Solas79">
// Copyright (c) Solas79. All rights reserved.
// </copyright>

namespace Jellyfin.Plugin.TmdbBoxsetMetadataForCollections
{
    using System;
    using MediaBrowser.Common.Plugins;

    /// <summary>
    /// Plugin entry point.
    /// </summary>
    public sealed class TmdbBoxsetMetadataForCollectionsPlugin : BasePlugin
    {
        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static TmdbBoxsetMetadataForCollectionsPlugin Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TmdbBoxsetMetadataForCollectionsPlugin"/> class.
        /// </summary>
        public TmdbBoxsetMetadataForCollectionsPlugin()
        {
            Instance = this;
        }

        /// <inheritdoc />
        public override string Name => "TMDb Boxset Metadata for Collections";

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("b11c1cde-4c6e-4c55-b4a5-5a4b95f7c801");

        /// <inheritdoc />
        public override string Description =>
            "Assigns TMDb boxset metadata to existing Jellyfin collections based on their movies.";
    }
}

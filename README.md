# do not install, betatest

# TMDb Boxset Metadata for Collections

For Jellyfin 10.11.x.

This plugin copies the TMDb Collection (BoxSet) ID from movies into the Jellyfin BoxSet item, allowing Jellyfin to fetch metadata and images for the collection.

## Install via Repository
Add this repository URL in Jellyfin:

https://raw.githubusercontent.com/Solas79/TmdbBoxsetMetadataForCollections/main/manifest.json

Then install from the Catalog and restart Jellyfin.

## Usage
- Run the scheduled task: "Scan library for TMDb Boxset IDs (Collections)"
- Or open a collection and run "Refresh Metadata" with "Replace images".

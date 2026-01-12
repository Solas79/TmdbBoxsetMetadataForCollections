# TMDb Boxset Metadata for Collections

This Jellyfin plugin assigns TMDb boxset metadata to existing Jellyfin collections.

If any movie inside a collection has a TMDb Collection ID, the plugin copies that ID to the collection.
Jellyfin then automatically downloads the correct boxset metadata (poster, description, etc).

Works with:
- CollectionByFolder
- Manual collections
- Existing libraries

No duplicate collections.
No API keys.
Fully Jellyfin-native.

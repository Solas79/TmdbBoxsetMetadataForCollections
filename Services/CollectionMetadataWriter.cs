using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using System.Threading;
using System.Threading.Tasks;

namespace TmdbBoxsetMetadataForCollections.Services;

public class CollectionMetadataWriter
{
    private readonly ILibraryManager _libraryManager;

    public CollectionMetadataWriter(ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
    }

    public async Task ApplyAsync(BoxSet col, TmdbBoxset data, CancellationToken ct)
    {
        col.Name = data.Name;
        col.Overview = data.Overview;

        await col.UpdateToRepositoryAsync(
            MediaBrowser.Controller.Entities.ItemUpdateType.MetadataEdit,
            ct
        );
    }
}

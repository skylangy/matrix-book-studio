using Matrix.Audio.Models;
using Raven.Client.Documents.Indexes;

namespace Matrix.Audio.Common.Services.RavenDb.Index;
public class AlbumsByTags : AbstractIndexCreationTask<Album>
{
    public AlbumsByTags()
    {
        Map = albums => from album in albums
                            //where album.Tags != null && album.Tags != string.Empty
                        select new
                        {
                            album.Id,
                            album.Tags
                        };
    }
}

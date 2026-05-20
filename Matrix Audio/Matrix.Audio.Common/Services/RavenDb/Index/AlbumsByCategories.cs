using Matrix.Audio.Models;
using Raven.Client.Documents.Indexes;

namespace Matrix.Audio.Common.Services.RavenDb.Index;
public class AlbumsByCategories : AbstractIndexCreationTask<Album>
{
    public AlbumsByCategories()
    {
        Map = albums => from album in albums
                            //where album.Categories != null && album.Categories != string.Empty
                        select new
                        {
                            album.Id,
                            album.Categories
                        };
    }
}

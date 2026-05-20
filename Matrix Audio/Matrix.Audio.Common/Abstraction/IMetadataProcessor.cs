namespace Matrix.Audio.Common.Abstraction;
public interface IMetadataProcessor
{
    Task<int> ScanArtistMetadata();

    Task<int> ScanAlbumMetadata();
}

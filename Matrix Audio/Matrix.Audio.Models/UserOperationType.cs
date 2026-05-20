namespace Matrix.Audio.Models;
public class UserOperationType
{
    public const string Login = nameof(Login);
    public const string Logout = nameof(Logout);
    public const string Favorite = nameof(Favorite);
    public const string UnFavorite = nameof(UnFavorite);
    public const string Like = nameof(Like);
    public const string Dislike = nameof(Dislike);
    public const string Comment = nameof(Comment);
    public const string DownloadEpisode = nameof(DownloadEpisode);
    public const string DownloadAlbum = nameof(DownloadAlbum);
    public const string PlayAlbum = nameof(PlayAlbum);
    public const string PlayEpisode = nameof(PlayEpisode);
    public const string PlayList = nameof(PlayList);
}

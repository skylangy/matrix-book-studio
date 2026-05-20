using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Linq;

namespace Matrix.Audio.Server.Common;

public static class ModelExtensions
{
    public static IEnumerable<ArtistViewModel> ToViewModels(this IEnumerable<Artist> artists,
        Func<IEnumerable<Artist>, IEnumerable<Artist>>? filter = null)
    {
        var viewModels = new List<ArtistViewModel>();

        if (filter != null)
        {
            artists = filter(artists);
        }
        foreach (var artist in artists)
        {
            var viewModel = artist.ToViewModel();
            viewModels.Add(viewModel);
        }

        return viewModels;
    }

    public static ArtistViewModel ToViewModel(this Artist artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        Alias = artist.Alias,
        Description = artist.Description,
        Image = artist.Image,
        DateCreated = artist.DateCreated,
        DateUpdated = artist.DateUpdated,
    };

    public static async Task<ArtistViewModel?> GetArtistViewModel(this IEntityRepository repository, string artistId)
    {
        var artist = await repository.GetAsync<Artist>(artistId);
        if (artist == null)
            return null;

        return new ArtistViewModel
        {
            Id = artist.Id,
            Name = artist.Name,
            Alias = artist.Alias,
            Description = artist.Description,
            Image = artist.Image,
            DateCreated = artist.DateCreated,
            DateUpdated = artist.DateUpdated
        };
    }

    public static async Task<IEnumerable<AlbumViewModel>> ToViewModels(
        this IEnumerable<Album> albums,
        Func<string, Task<ArtistViewModel?>> artistRetrever,
        Func<string, Task<int>>? episodeCountRetriever = null)

    {
        var viewModels = new List<AlbumViewModel>();
        foreach (var album in albums)
        {
            var viewModel = await album.ToViewModel(artistRetrever, episodeCountRetriever);
            if (viewModel != null)
            {
                viewModels.Add(viewModel);
            }
        }

        return viewModels;
    }

    public static IEnumerable<AlbumViewModel> ToViewModels(this IEnumerable<Album> albums, ArtistViewModel artist)
    {
        var viewModels = new List<AlbumViewModel>();
        foreach (var album in albums)
        {
            var viewModel = album.ToViewModel(artist);
            if (viewModel != null)
            {
                viewModels.Add(viewModel);
            }
        }

        return viewModels;
    }

    public static async Task<AlbumViewModel?> ToViewModel(this Album album,
        Func<string, Task<ArtistViewModel?>> artistRetrever,
        Func<string, Task<int>>? episodeCountRetriever = null)
    {
        if (album == null || album.ArtistId == null)
        {
            return null;
        }

        var artist = await artistRetrever(album.ArtistId);
        if (artist == null)
            return null;

        return new AlbumViewModel
        {
            Id = album.Id,
            Title = album.Title,
            Subtitle = album.Subtitle,
            Description = album.Description,
            Artist = artist,
            ArtistName = album.Artist,
            ArtistId = album.ArtistId,
            PlayCount = album.PlayCount,
            LikeCount = album.LikeCount,
            DownloadCount = album.DownloadCount,
            CommentCount = album.CommentCount,
            EpisodeCount = episodeCountRetriever != null ? await episodeCountRetriever(album.Id) : 0,
            ImageWideSplash = album.ImageWideSplashId,
            ImageSquareSplash = album.ImageSquareSplashId,
            Tags = string.Join(",", album.Tags),
            Categories = string.Join(",", album.Categories),
            Status = album.Status,
            DateCreated = album.DateCreated,
            DateUpdated = album.DateUpdated
        };
    }

    public static AlbumViewModel ToViewModel(this Album album, ArtistViewModel artist)
    {
        return new AlbumViewModel
        {
            Id = album.Id,
            Title = album.Title,
            Subtitle = album.Subtitle,
            Description = album.Description,
            Artist = artist,
            ImageWideSplash = album.ImageWideSplashId,
            ImageSquareSplash = album.ImageSquareSplashId,
            Tags = string.Join(",", album.Tags),
            Categories = string.Join(",", album.Categories),
            Status = album.Status,
            DateCreated = album.DateCreated,
            DateUpdated = album.DateUpdated,
            EpisodeCount = album.Episodes.Count
        };
    }

    public static AlbumViewModel ToViewModel(this Album album)
    {
        return new AlbumViewModel
        {
            Id = album.Id,
            Title = album.Title,
            Subtitle = album.Subtitle,
            Description = album.Description,
            ImageWideSplash = album.ImageWideSplashId,
            ImageSquareSplash = album.ImageSquareSplashId,
            Tags = string.Join(",", album.Tags),
            Categories = string.Join(",", album.Categories),
            Status = album.Status,
            DateCreated = album.DateCreated,
            DateUpdated = album.DateUpdated,
            EpisodeCount = album.Episodes.Count,
            Level = album.Level
        };
    }

    public static async Task<IEnumerable<AlbumViewModel>> GetAlbumsForArtist(this IQueryableRepository repository, Artist artist)
    {
        var artistViewModel = artist.ToViewModel();
        var albums = await repository.QueryAsync(session => session.Query<Album>().Where(x => x.ArtistId == artist.Id));


        var viewModels = albums.ToViewModels(artistViewModel);
        return viewModels;
    }

    public static EpisodeViewModel ToViewModel(this Episode episode, Album? album = null, bool full = true)
    {
        var length = Math.Min(100, episode.Content?.Length ?? 0);

        return new EpisodeViewModel
        {
            Id = episode.Id,
            Title = episode.Title,
            Content = full ? episode.Content : episode.Content?[..length],
            Duration = episode.Duration,
            FileLength = episode.FileLength,
            PlayCount = episode.PlayCount,
            LikeCount = episode.LikeCount,
            DownloadCount = episode.DownloadCount,
            DateCreated = episode.DateCreated,
            DateUpdated = episode.DateUpdated,
            AlbumId = album?.Id,
            AlbumTitle = album?.Title,
            ArtistId = album?.ArtistId,
            ArtistName = album?.Artist
        };
    }

    public static IEnumerable<EpisodeViewModel> ToViewModels(this IEnumerable<Episode> episodes, Album album, bool full = true)
    {
        var viewModels = new List<EpisodeViewModel>();
        foreach (var episode in episodes)
        {
            var viewModel = episode.ToViewModel(album, full);

            viewModels.Add(viewModel);
        }
        return viewModels;
    }


    public static IActionResult GetEpisodeStreamAsync(this Episode episode,
        string booksLocation, string artistName, string albumTitle, ILogger logger)
    {
        var fileName = $"{episode.Title}.mp3";
        var filePath = Path.Combine(booksLocation, artistName, albumTitle, "mp3", fileName);
        logger.LogInformation("Audio File Path: {filePath}", filePath);
        if (File.Exists(filePath))
        {
            return new PhysicalFileResult(filePath, "audio/mpeg")
            {
                FileDownloadName = fileName,
                EnableRangeProcessing = true
            };
        }

        logger.LogWarning("Episode '{}' is not found at {}", episode.Title, filePath);

        return new NotFoundResult();
    }

    public static UserViewModel ToViewModel(this User user, UserProfile? profile) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        ImageId = user.ImageId,
        Level = user.Level ?? 1000,
        FirstName = profile?.FirstName ?? string.Empty,
        LastName = profile?.LastName ?? string.Empty,
        Bio = profile?.Bio ?? string.Empty,
        DateCreated = user.DateCreated,
        DateUpdated = user.DateUpdated
    };

    public static UserViewModel ToViewModel(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        ImageId = user.ImageId,
        Level = user.Level ?? 1000,
        DateCreated = user.DateCreated,
        DateUpdated = user.DateUpdated
    };

    public static IEnumerable<UserViewModel> ToViewModels(this IEnumerable<User> users,
        Func<string, UserProfile> profileReriever)
    {
        foreach (var user in users)
        {
            yield return user.ToViewModel(profileReriever?.Invoke(user.Id));
        }
    }

    //public static async Task<IEnumerable<AlbumViewModel>> QueryAlbumsByIds(this IQueryableRepository queryableRepository,
    //                    IEnumerable<string> albumIds,
    //                    Func<string, Task<int>>? episodeCountRetriever = null)
    //{
    //    var albums = await queryableRepository.QueryAsync(session => session.Query<Album>().Where(x => x.Id.In(albumIds)));

    //    var artistIds = albums.Where(x => !string.IsNullOrWhiteSpace(x.ArtistId)).Select(x => x.ArtistId).Distinct().ToList();
    //    var artists = await queryableRepository.QueryAsync(session => session.Query<Artist>().Where(x => x.Id.In(artistIds)));

    //    var viewModels = new List<AlbumViewModel>();
    //    foreach (var album in albums)
    //    {
    //        var artist = artists.FirstOrDefault(a => a.Id == album.ArtistId);
    //        var artistViewModel = artist?.ToViewModel();

    //        var albumViewModel = await album.ToViewModel(
    //            artistId => Task.FromResult(artistViewModel),
    //            episodeCountRetriever);
    //        if (albumViewModel != null)
    //        {
    //            viewModels.Add(albumViewModel);
    //        }
    //    }
    //    return viewModels;
    //}


    public static async Task<IEnumerable<AlbumViewModel>> QueryAlbumsByIds(this IEntityRepository entityRepository,
                        IEnumerable<string> albumIds)
    {
        var viewModels = new List<AlbumViewModel>();

        foreach (var albumId in albumIds)
        {
            var album = await entityRepository.GetAsync<Album>(albumId);
            if (album == null)
                continue;

            var artist = await entityRepository.GetAsync<Artist>(album.ArtistId!);
            var artistViewModel = artist?.ToViewModel();

            var albumViewModel = album.ToViewModel(artistViewModel!);
            if (albumViewModel != null)
            {
                viewModels.Add(albumViewModel);
            }
        }

        return viewModels;
    }


    public static async Task<AlbumCollectionViewModel> ToViewModel(this IEntityRepository entityRepository, AlbumCollection albumCollection)
    {
        var albumViewModels = new List<AlbumViewModel>();

        var ablumIds = albumCollection.GetSortedAlbumIds();
        foreach (var albumId in ablumIds)
        {
            var album = await entityRepository.QueryOneAsync(session => session.Query<Album>().Where(x => x.Id == albumId));
            if (album == null)
                continue;

            var viewModel = album.ToViewModel();
            if (viewModel != null)
            {
                albumViewModels.Add(viewModel);
            }
        }

        return new AlbumCollectionViewModel
        {
            Id = albumCollection.Id,
            Name = albumCollection.Name,
            Description = albumCollection.Description,
            Image = albumCollection.Image,
            Count = albumViewModels.Count,
            Albums = albumViewModels,
            DateCreated = albumCollection.DateCreated,
            DateUpdated = albumCollection.DateUpdated
        };
    }

    public static async Task<IEnumerable<AlbumCollectionViewModel>> ToViewModels(this IEntityRepository entityRepository, IEnumerable<AlbumCollection> albumCollections)
    {
        var viewModels = new List<AlbumCollectionViewModel>();
        foreach (var albumCollection in albumCollections)
        {
            var viewModel = await entityRepository.ToViewModel(albumCollection);
            viewModels.Add(viewModel);
        }
        return viewModels;
    }

    public static async Task<IEnumerable<AlbumCollectionViewModel>> GetPagedAlbumCollections(this IEntityRepository entityRepository, int page, int pageSize)
    {
        var albumCollections = await entityRepository.QueryAsync(session => session.Query<AlbumCollection>()
                                                                             .OrderByDescending(x => x.DateUpdated)
                                                                             .Skip((page - 1) * pageSize)
                                                                             .Take(pageSize));
        return await entityRepository.ToViewModels(albumCollections);
    }

    public static async Task<AlbumCollectionViewModel> GetAlbumCollection(this IEntityRepository entityRepository, string id)
    {
        var albumCollection = await entityRepository.GetAsync<AlbumCollection>(id);
        if (albumCollection == null)
        {
            return new AlbumCollectionViewModel();
        }
        return await entityRepository.ToViewModel(albumCollection);
    }
}

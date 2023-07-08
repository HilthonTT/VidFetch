using VidFetchLibrary.Client;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Generics;
public class GenericLoadHelper<TData> : IGenericLoadHelper<TData> where TData : class
{
    private readonly IYoutube _youtube;
    private readonly IVideoLibrary _videoLibrary;

    public GenericLoadHelper(
        IYoutube youtube,
        IVideoLibrary videoLibrary)
    {
        _youtube = youtube;
        _videoLibrary = videoLibrary;
    }

    public async Task<List<TData>> LoadDataAsync(string url, int loadedItems)
    {
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                return await LoadChannelAsync(url, loadedItems);
            case Type videoModelType when videoModelType == typeof(VideoModel):
                return await LoadVideoAsync(url, loadedItems);
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                return await LoadPlaylistAsync(url, loadedItems);
            default:
                return default;
        }
    }

    private async Task<List<TData>> LoadChannelAsync(string url, int loadedItems)
    {
        var channel = await _youtube.GetChannelAsync(url);

        if (IsDataNotLoaded(channel.ChannelId))
        {
            _videoLibrary.Channels.Add(channel);
        }

        return _videoLibrary.Channels.Take(loadedItems).ToList() as List<TData>;
    }

    private async Task<List<TData>> LoadVideoAsync(string url, int loadedItems)
    {
        var video = await _youtube.GetVideoAsync(url);

        if (IsDataNotLoaded(video.VideoId))
        {
            _videoLibrary.Videos.Add(video);
        }

        return _videoLibrary.Videos.Take(loadedItems).ToList() as List<TData>;
    }

    private async Task<List<TData>> LoadPlaylistAsync(string url, int loadedItems)
    {
        var playlist = await _youtube.GetPlaylistAsync(url);

        if (IsDataNotLoaded(playlist.PlaylistId))
        {
            _videoLibrary.Playlists.Add(playlist);
        }

        return _videoLibrary.Playlists.Take(loadedItems).ToList() as List<TData>;
    }

    private bool IsDataNotLoaded(string dataId)
    {
        switch (typeof(TData))
        {
            case Type channel when channel == typeof(ChannelModel):
                return _videoLibrary.Channels.Any(c => c.ChannelId == dataId) is false;
            case Type video when video == typeof(VideoModel):
                return _videoLibrary.Videos.Any(v => v.VideoId == dataId) is false;
            case Type playlist when playlist == typeof(PlaylistModel):
                return _videoLibrary.Playlists.Any(p => p.PlaylistId == dataId) is false;
            default:
                return false;
        }
    }
}

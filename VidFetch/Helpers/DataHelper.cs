using VidFetchLibrary.Client;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Language;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Helpers;
public class DataHelper<TData> : IDataHelper<TData> where TData : class
{
    private readonly IYoutube _youtube;
    private readonly ISnackbarHelper _snackbarHelper;
    private readonly IVideoData _videoData;
    private readonly IChannelData _channelData;
    private readonly IPlaylistData _playlistData;
    private readonly IVideoLibrary _videoLibrary;
    private readonly ILanguageExtension _languageExtension;

    public DataHelper(
        IYoutube youtube,
        ISnackbarHelper snackbarHelper,
        IVideoData videoData,
        IChannelData channelData,
        IPlaylistData playlistData,
        IVideoLibrary videoLibrary,
        ILanguageExtension languageExtension)
    {
        _youtube = youtube;
        _snackbarHelper = snackbarHelper;
        _videoData = videoData;
        _channelData = channelData;
        _playlistData = playlistData;
        _videoLibrary = videoLibrary;
        _languageExtension = languageExtension;
    }

    public async Task DownloadAllVideosAsync(
        List<TData> datas,
        Progress<double> progress,
        CancellationToken token,
        Action<TData> RemoveData = default)
    {
        if (IsVideoModel() is false)
        {
            return;
        }

        foreach (var data in datas)
        {
            var video = data as VideoModel;

            await DownloadVideoAsync(video, progress, token);

            RemoveData(data);
        }

        string videos = GetDictionary()[KeyWords.Videos];
        _snackbarHelper.ShowSuccessfullyDownloadedMessage(videos);
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

    public async Task DeleteAllAsync(List<TData> datas, Action<TData> RemoveData)
    {
        foreach (var d in datas)
        {
            RemoveData(d);
            await DeleteDataAsync(d);
        }

        string className = GetName() + "s";
        _snackbarHelper.ShowSuccessfullyDeleteMessage(className);
    }

    public async Task DeleteDataAsync(TData data)
    {
        switch (data.GetType())
        {
            case Type videoModelType when videoModelType == typeof(VideoModel):
                await _videoData.DeleteVideoAsync(data as VideoModel);
                break;

            case Type channelModelType when channelModelType == typeof(ChannelModel):
                await _channelData.DeleteChannelAsync(data as ChannelModel);
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                await _playlistData.DeletePlaylistAsync(data as PlaylistModel);
                break;
            default:
                break;
        }
    }

    public async Task UpdateAllAsync(List<TData> datas, Action<TData> RemoveData, CancellationToken token)
    {
        foreach (var d in datas)
        {
            token.ThrowIfCancellationRequested();
            await UpdateDataAsync(d, RemoveData, token);
        }

        string name = GetName() + "s";
        _snackbarHelper.ShowSuccessfullyUpdatedDataMessage(name);
    }

    public async Task SaveAllAsync(List<TData> datas, CancellationToken token)
    {
        foreach(var d in datas)
        {
            token.ThrowIfCancellationRequested();
            await SaveAsync(d);
        }

        string className = GetName();
        _snackbarHelper.ShowSuccessfullySavedMessage(className);
    }

    public string GetName()
    {
        string name = typeof(TData) switch
        {
            Type channelModelType when channelModelType == typeof(ChannelModel) => GetDictionary()[KeyWords.Channel],
            Type playlistModelType when playlistModelType == typeof(PlaylistModel) => GetDictionary()[KeyWords.Playlist],
            Type videoModelType when videoModelType == typeof(VideoModel) => GetDictionary()[KeyWords.Video],
            _ => "",
        };
        return name;
    }

    public void ClearDatas()
    {
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                _videoLibrary.Channels.Clear();
                break;
            case Type videoModelType when videoModelType == typeof(VideoModel):
                _videoLibrary.Videos.Clear();
                break;
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                _videoLibrary.Playlists.Clear();
                break;
            default:
                break;
        }
    }

    public void RemoveData(TData data)
    {
        switch (data)
        {
            case ChannelModel channel:
                _videoLibrary.Channels.Remove(channel);
                break;
            case PlaylistModel playlist:
                _videoLibrary.Playlists.Remove(playlist);
                break;
            case VideoModel video:
                _videoLibrary.Videos.Remove(video);
                break;
        }
    }

    private async Task SaveAsync(TData data)
    {
        switch (data.GetType())
        {
            case Type video when video == typeof(VideoModel):
                var v = data as VideoModel;
                await _videoData.SetVideoAsync(v.Url, v.VideoId);
                break;

            case Type channel when channel == typeof(ChannelModel):
                var c = data as ChannelModel;
                await _channelData.SetChannelAsync(c.Url, c.ChannelId);
                break;

            case Type playlist when playlist == typeof(PlaylistModel):
                var p = data as PlaylistModel;
                await _playlistData.SetPlaylistAsync(p.Url, p.PlaylistId);
                break;

            default:
                break;
        }
    }

    private async Task UpdateDataAsync(TData data, Action<TData> RemoveData, CancellationToken token)
    {
        switch (data.GetType())
        {
            case Type video when video == typeof(VideoModel):
                await UpdateVideoAsync(data, RemoveData, token);
                break;

            case Type channel when channel == typeof(ChannelModel):
                await UpdateChannelAsync(data, RemoveData, token);
                break;

            case Type playlist when playlist == typeof(PlaylistModel):
                await UpdatePlaylistAsync(data, RemoveData, token);
                break;
            default:
                break;
        }
    }

    private async Task DownloadVideoAsync(VideoModel video, Progress<double> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        _snackbarHelper.ShowCurrentlyDownloading(video.Title);
        await _youtube.DownloadVideoAsync(video.Url, progress, token);

        _snackbarHelper.ShowSuccessfullyDownloadedMessage();
    }

    private async Task UpdateVideoAsync(TData video, Action<TData> RemoveData, CancellationToken token)
    {
        var convertedVideo = video as VideoModel;
        var newVideo = await _youtube.GetVideoAsync(convertedVideo.Url, token);

        if (newVideo is null)
        {
            RemoveData(video);
            string v = GetDictionary()
                [KeyWords.Video];

            _snackbarHelper.ShowSuccessfullyUpdatedDataMessage(v);

            await _videoData.DeleteVideoAsync(convertedVideo);
        }
        else
        {
            await _videoData.SetVideoAsync(convertedVideo.Url, convertedVideo.VideoId);
        }
    }

    private async Task UpdateChannelAsync(TData channel, Action<TData> RemoveData, CancellationToken token)
    {
        var convertedChannel = channel as ChannelModel;
        var newChannel = await _youtube.GetChannelAsync(convertedChannel.Url, token);
        if (newChannel is null)
        {
            RemoveData(channel);

            _snackbarHelper.ShowNoLongerExistsMessage();

            await _channelData.DeleteChannelAsync(convertedChannel);
        }
        else
        {
            await _channelData.SetChannelAsync(convertedChannel.Url, convertedChannel.ChannelId);
        }
    }

    private async Task UpdatePlaylistAsync(TData playlist, Action<TData> RemoveData, CancellationToken token)
    {
        var convertedPlaylist = playlist as PlaylistModel;
        var newPlaylist = await _youtube.GetPlaylistAsync(convertedPlaylist.Url, token);
        if (newPlaylist is null)
        {
            RemoveData(playlist);

            _snackbarHelper.ShowNoLongerExistsMessage();

            await _playlistData.DeletePlaylistAsync(convertedPlaylist);
        }
        else
        {
            await _playlistData.SetPlaylistAsync(convertedPlaylist.Url, convertedPlaylist.PlaylistId);
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
                return _videoLibrary.Videos.Any(v => v.VideoId == dataId) is false; ;
            case Type playlist when playlist == typeof(PlaylistModel):
                return _videoLibrary.Playlists.Any(p => p.PlaylistId == dataId) is false;
            default:
                return false;
        }
    }

    private static bool IsVideoModel()
    {
        return typeof(TData) == typeof(VideoModel);
    }

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = _languageExtension.GetDictionary();
        return dictionary;
    }
}

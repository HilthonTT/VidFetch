using VidFetch.Helpers;
using VidFetchLibrary.Client;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Language;
using VidFetchLibrary.Models;

namespace VidFetch.Generics;
public class GenericUpdateHelper<TData> : IGenericUpdateHelper<TData> where TData : class
{
    private readonly IYoutube _youtube;
    private readonly ISnackbarHelper _snackbarHelper;
    private readonly ILanguageExtension _languageExtension;
    private readonly IVideoData _videoData;
    private readonly IChannelData _channelData;
    private readonly IPlaylistData _playlistData;

    public GenericUpdateHelper(
        IYoutube youtube,
        ISnackbarHelper snackbarHelper,
        ILanguageExtension languageExtension,
        IVideoData videoData,
        IChannelData channelData,
        IPlaylistData playlistData)
    {
        _youtube = youtube;
        _snackbarHelper = snackbarHelper;
        _languageExtension = languageExtension;
        _videoData = videoData;
        _channelData = channelData;
        _playlistData = playlistData;
    }

    public async Task UpdateAllAsync(List<TData> datas, CancellationToken token, Action<TData> RemoveData)
    {
        foreach (var data in datas)
        {
            token.ThrowIfCancellationRequested();

            await UpdateDataAsync(data, RemoveData, token);
        }
    }

    public async Task UpdateDataAsync(TData data, Action<TData> RemoveData, CancellationToken token)
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

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = _languageExtension.GetDictionary();
        return dictionary;
    }
}

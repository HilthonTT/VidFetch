using VidFetch.Helpers;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Language;
using VidFetchLibrary.Models;

namespace VidFetch.Generics;
public class GenericSaveHelper<TData> : IGenericSaveHelper<TData> where TData : class
{
    private readonly IVideoData _videoData;
    private readonly IChannelData _channelData;
    private readonly IPlaylistData _playlistData;
    private readonly ISnackbarHelper _snackbarHelper;
    private readonly ILanguageExtension _languageExtension;

    public GenericSaveHelper(
        IVideoData videoData,
        IChannelData channelData,
        IPlaylistData playlistData,
        ISnackbarHelper snackbarHelper,
        ILanguageExtension languageExtension)
    {
        _videoData = videoData;
        _channelData = channelData;
        _playlistData = playlistData;
        _snackbarHelper = snackbarHelper;
        _languageExtension = languageExtension;
    }

    public async Task SaveAllAsync(List<TData> datas, CancellationToken token)
    {
        foreach (var d in datas)
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
            Type channelModelType when channelModelType == typeof(ChannelModel) => GetDictionary()
                [KeyWords.Channels],

            Type playlistModelType when playlistModelType == typeof(PlaylistModel) => GetDictionary()
                [KeyWords.Playlists],

            Type videoModelType when videoModelType == typeof(VideoModel) => GetDictionary()
                [KeyWords.Videos],
            _ => "",
        };

        return name.ToLower();
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

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = _languageExtension.GetDictionary();
        return dictionary;
    }
}

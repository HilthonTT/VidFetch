using VidFetch.Helpers;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Language;
using VidFetchLibrary.Models;

namespace VidFetch.Generics;
public class GenericDeleteHelper<TData> : IGenericDeleteHelper<TData> where TData : class
{
    private readonly IVideoData _videoData;
    private readonly IChannelData _channelData;
    private readonly IPlaylistData _playlistData;
    private readonly ISnackbarHelper _snackbarHelper;
    private readonly ILanguageExtension _languageExtension;

    public GenericDeleteHelper(
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

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = _languageExtension.GetDictionary();
        return dictionary;
    }
}

using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Generics;
public class GenericGeneralHelper<TData> : IGenericGeneralHelper<TData> where TData : class
{
    private readonly IGenericDeleteHelper<TData> _deleteHelper;
    private readonly IGenericDownloadHelper<TData> _downloadHelper;
    private readonly IGenericLoadHelper<TData> _loadHelper;
    private readonly IGenericUpdateHelper<TData> _updateHelper;
    private readonly IGenericSaveHelper<TData> _saveHelper;
    private readonly IVideoLibrary _videoLibrary;

    public GenericGeneralHelper(
        IGenericDeleteHelper<TData> deleteHelper,
        IGenericDownloadHelper<TData> downloadHelper,
        IGenericLoadHelper<TData> loadHelper,
        IGenericUpdateHelper<TData> updateHelper,
        IGenericSaveHelper<TData> saveHelper,
        IVideoLibrary videoLibrary)
    {
        _deleteHelper = deleteHelper;
        _downloadHelper = downloadHelper;
        _loadHelper = loadHelper;
        _updateHelper = updateHelper;
        _saveHelper = saveHelper;
        _videoLibrary = videoLibrary;
    }

    public async Task DeleteAllAsync(List<TData> datas, Action<TData> removeData)
    {
        await _deleteHelper.DeleteAllAsync(datas, removeData);
    }

    public async Task DeleteAsync(TData data)
    {
        await _deleteHelper.DeleteDataAsync(data);
    }

    public async Task DownloadAllAsync(
        List<TData> datas,
        Progress<double> progress,
        CancellationToken token,
        Action<TData> removeData = default)
    {
        await _downloadHelper.DownloadAllVideosAsync(datas, progress, token, removeData);
    }

    public async Task<List<TData>> LoadDataAsync(string url, int loadedItems)
    {
        return await _loadHelper.LoadDataAsync(url, loadedItems);
    }

    public async Task UpdateAllDataAsync(List<TData> datas, CancellationToken token, Action<TData> removeData = default)
    {
        await _updateHelper.UpdateAllAsync(datas, token, removeData);
    }

    public async Task UpdateDataAsync(TData data, Action<TData> removeData, CancellationToken token)
    {
        await _updateHelper.UpdateDataAsync(data, removeData, token);
    }

    public async Task SaveDataAsync(List<TData> datas, CancellationToken token)
    {
        await _saveHelper.SaveAllAsync(datas, token);
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

    public string GetName()
    {
        return _saveHelper.GetName();
    }
}

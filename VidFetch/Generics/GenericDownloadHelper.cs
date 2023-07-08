using VidFetch.Helpers;
using VidFetchLibrary.Client;
using VidFetchLibrary.Language;
using VidFetchLibrary.Models;

namespace VidFetch.Generics;
public class GenericDownloadHelper<TData> : IGenericDownloadHelper<TData> where TData : class
{
    private readonly IYoutube _youtube;
    private readonly ISnackbarHelper _snackbarHelper;
    private readonly ILanguageExtension _languageExtension;

    public GenericDownloadHelper(
        IYoutube youtube,
        ISnackbarHelper snackbarHelper,
        ILanguageExtension languageExtension)
    {
        _youtube = youtube;
        _snackbarHelper = snackbarHelper;
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

    private async Task DownloadVideoAsync(VideoModel video, Progress<double> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        _snackbarHelper.ShowCurrentlyDownloading(video.Title);
        await _youtube.DownloadVideoAsync(video.Url, progress, token);

        _snackbarHelper.ShowSuccessfullyDownloadedMessage();
    }

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = _languageExtension.GetDictionary();
        return dictionary;
    }

    private static bool IsVideoModel()
    {
        return typeof(TData) == typeof(VideoModel);
    }
}

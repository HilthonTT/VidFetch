using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Language;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Components;

public partial class VideoComponent
{
    [Parameter]
    [EditorRequired]
    public VideoModel Video { get; set; }

    [Parameter]
    public int CardSize { get; set; } = 12;

    [Parameter]
    [EditorRequired]
    public EventCallback<VideoModel> RemoveEvent { get; set; }

    [Parameter]
    public int Index { get; set; }

    private SettingsLibrary _settings;
    private CancellationTokenSource _tokenSource;
    private bool _isDownloading = false;
    private bool _isSaved = false;
    private double _progress = 0;

    protected override async Task OnInitializedAsync()
    {
        _settings = await settingsData.GetSettingsAsync();
        _isSaved = await videoData.VideoExistsAsync(Video.Url, Video.VideoId);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {    
            await LoadVideoData();
        }
    }

    private async Task LoadVideoData()
    {
        var videoTasks = new List<Task>
        {
            LoadAuthorThumbnail(),
        };

        await Task.WhenAll(videoTasks);
    }

    private async Task LoadAuthorThumbnail()
    {

        bool isAuthorThumbnailEmpty = string.IsNullOrWhiteSpace(Video.AuthorThumbnailUrl);

        if (isAuthorThumbnailEmpty)
        {
            var channel = await youtube.GetChannelAsync(Video.AuthorUrl);
            Video.AuthorThumbnailUrl = channel.ThumbnailUrl;

            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task DownloadVideo()
    {
        try
        {
            _isDownloading = true;

            ShowFFmpegWarningIfNeeded();

            var token = tokenHelper.InitializeToken(ref _tokenSource);

            var progress = new Progress<double>(async val =>
            {
                await UpdateProgress(val);
            });

            await youtube.DownloadVideoAsync(Video.Url, progress, token);

            await InvokeAsync(() =>
            {
                snackbarHelper.ShowSuccessfullyDownloadedMessage(Video.Title);
            });
        }
        catch (OperationCanceledException)
        {
            await InvokeAsync(snackbarHelper.ShowErrorOperationCanceledMessage);
        }
        catch
        {
            await InvokeAsync(snackbarHelper.ShowErrorDownloadMessage);
        }
        finally
        {
            CancelVideoDownload();
        }
    }

    private void ShowFFmpegWarningIfNeeded()
    {
        if (IsFFmpegInvalid() is false)
        {
            return;
        }

        snackbarHelper.ShowFfmpegError();
    }

    private async Task UpdateProgress(double value)
    {
        if (Math.Abs(value - _progress) < 0.1)
            return;

        _progress = value;
        await InvokeAsync(StateHasChanged);
    }

    private async Task SaveVideo()
    {
        if (_isSaved is false)
        {
            _isSaved = true;
            await videoData.SetVideoAsync(Video.Url, Video.VideoId);

            await InvokeAsync(() =>
            {
                snackbarHelper.ShowSuccessfullySavedMessage(Video.Title);
            });
        }
    }

    private async Task Remove()
    {
        videoData.RemoveVideoCache(Video);
        await RemoveEvent.InvokeAsync(Video);
    }

    private async Task OpenUrl(string text)
    {
        await launcher.OpenAsync(text);
    }

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
        _isDownloading = false;
        _progress = 0;
    }

    private void LoadWatchPage()
    {
        string encodedUrl = Uri.EscapeDataString(Video.Url);
        navManager.NavigateTo($"/Watch/{encodedUrl}");
    }

    private string GetSaveVideoText()
    {
        string saveText = GetDictionary()[KeyWords.Save];
        string videoText = GetDictionary()[KeyWords.Video];

        return $"{saveText} {videoText}";
    }

    private Dictionary<KeyWords, string> GetDictionary(string text = "")
    {
        var dictionary = languageExtension.GetDictionary(text);
        return dictionary;
    }

    private bool IsFFmpegInvalid()
    {
        bool isFFmpegEmpty = string.IsNullOrWhiteSpace(_settings.FfmpegPath) is false;
        bool ffmpPegDoesNotExist = File.Exists(_settings.FfmpegPath) is false;

        if (isFFmpegEmpty && ffmpPegDoesNotExist)
        {
            return true;
        }

        return false;
    }
}
using Microsoft.AspNetCore.Components;
using MudBlazor;
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

    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
    private CancellationTokenSource _tokenSource;
    private bool _isDownloading = false;
    private bool _isSaved = false;
    private double _progress = 0;

    protected override async Task OnInitializedAsync()
    {
        _isSaved = await videoData.VideoExistsAsync(Video.Url, Video.VideoId);
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadNullData();
    }

    private async Task DownloadVideo()
    {
        _isDownloading = true;

        if (IsFFmpegPathInvalid())
        {
            snackbar.Add(FfmpegErrorMessage, Severity.Warning);
        }

        var cancellationToken = tokenHelper.InitializeToken(ref _tokenSource);
        
        var progressReporter = new Progress<double>(value =>
        {
            _progress = value;
            StateHasChanged();
        });

        await youtube.DownloadVideoAsync(
            Video.Url,
            progressReporter,
            cancellationToken);
        
        AddSnackbar();
        CancelVideoDownload();
    }

    private async Task SaveVideo()
    {
        if (_isSaved is false)
        {
            await videoData.SetVideoAsync(Video.Url, Video.VideoId);
            snackbar.Add($"Successfully saved {Video.Title}");
            _isSaved = true;
        }
    }

    private async Task Remove()
    {
        await RemoveEvent.InvokeAsync(Video);
    }

    private async Task OpenUrl(string text)
    {
        await launcher.OpenAsync(text);
    }

    private async Task LoadNullData()
    {
        bool isAuthorThumbnailEmpty = string.IsNullOrWhiteSpace(Video.AuthorThumbnailUrl);

        if (isAuthorThumbnailEmpty)
        {
            string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";

            var channel = await youtube.GetChannelAsync(Video.AuthorUrl);
            string channelThumbnail = string.IsNullOrWhiteSpace(channel.ThumbnailUrl) ? defaultUrl : channel.ThumbnailUrl;
            Video.AuthorThumbnailUrl =  channelThumbnail;
        }
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

    private void AddSnackbar()
    {
        snackbar.Add($"Successfully downloaded {Video.Title}", Severity.Normal);
    }

    private bool IsFFmpegPathInvalid()
    {
        string path = settingsLibrary.FfmpegPath;
        bool isPathNotEmptyOrNull = path is not null;
        bool FileExists = File.Exists(path);

        if (isPathNotEmptyOrNull && FileExists)
        {
            return true;
        }

        return false;
    }
}
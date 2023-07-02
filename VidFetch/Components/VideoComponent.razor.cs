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
            string channelThumbnail = string.IsNullOrWhiteSpace(channel.ThumbnailUrl) ? "" : channel.ThumbnailUrl;
            Video.AuthorThumbnailUrl = channelThumbnail;
        }
    }

    private async Task DownloadVideo()
    {
        try
        {
            _isDownloading = true;

            if (IsFFmpegInvalid())
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var token = tokenHelper.InitializeToken(ref _tokenSource);

            var progress = new Progress<double>(value =>
            {
                UpdateProgress(ref _progress, value);
            });

            await youtube.DownloadVideoAsync(Video.Url, progress, token);

            AddSnackbar();
            CancelVideoDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        if (Math.Abs(value - progressVariable) < 0.1)
            return;

        progressVariable = value;
        StateHasChanged();
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
        videoData.RemoveVideoCache(Video.VideoId);
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

    private void AddSnackbar()
    {
        snackbar.Add($"Successfully downloaded {Video.Title}", Severity.Normal);
    }

    private bool IsFFmpegInvalid()
    {
        bool isFFmpegEmpty = string.IsNullOrWhiteSpace(settingsLibrary.FfmpegPath) is false;
        bool ffmpPegDoesNotExist = File.Exists(settingsLibrary.FfmpegPath) is false;

        if (isFFmpegEmpty && ffmpPegDoesNotExist)
        {
            return true;
        }

        return false;
    }
}
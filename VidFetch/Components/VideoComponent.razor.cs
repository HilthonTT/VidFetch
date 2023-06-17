using Microsoft.AspNetCore.Components;
using MudBlazor;
using YoutubeExplode.Channels;
using YoutubeExplode.Videos;

namespace VidFetch.Components;

public partial class VideoComponent
{
    [Parameter]
    [EditorRequired]
    public string Url { get; set; }

    [Parameter]
    public int CardSize { get; set; } = 12;

    [Parameter]
    [EditorRequired]
    public string SelectedExtension { get; set; }

    [Parameter]
    [EditorRequired]
    public string SelectedPath { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<Video> RemoveEvent { get; set; }

    [Parameter]
    public int Index { get; set; }

    private Video video;
    private Channel channel;
    private bool isDownloading = false;
    private bool isDownloadSuccessful = false;
    private bool isSaved = false;
    private double progress = 0;
    private CancellationTokenSource tokenSource;
    protected override async Task OnInitializedAsync()
    {
        video = await youtubeDownloader.GetVideoAsync(Url);
        if (video is not null)
        {
            isSaved = await videoData.VideoExistAsync(Url, video.Id);
            channel = await youtubeDownloader.GetChannelAsync(video.Author.ChannelUrl);
        }
    }

    private async Task DownloadVideo()
    {
        isDownloading = true;
        var cancellationToken = tokenHelper.InitializeToken(ref tokenSource);
        
        var progressReporter = new Progress<double>(value =>
        {
            progress = value;
            StateHasChanged();
        });

        await youtubeDownloader.DownloadVideoAsync(
            video.Url,
            SelectedPath,
            SelectedExtension,
            progressReporter,
            cancellationToken);
        
        AddSnackbar();
        CancelVideoDownload();
        isDownloadSuccessful = true;
    }

    private async Task SaveVideo()
    {
        if (isSaved is false)
        {
            await videoData.SetVideoAsync(video.Url, video.Id);
            isSaved = true;
        }
    }

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref tokenSource);
        isDownloading = false;
        progress = 0;
    }

    private void LoadWatchPage()
    {
        string encodedUrl = Uri.EscapeDataString(video.Url);
        navManager.NavigateTo($"/Watch/{encodedUrl}");
    }

    private void AddSnackbar()
    {
        snackbar.Add($"Successfully downloaded {video.Title}", Severity.Normal);
    }

    private async Task Remove()
    {
        await RemoveEvent.InvokeAsync(video);
    }

    private async Task OpenFolderLocation()
    {
        await folderHelper.OpenFolderLocationAsync(SelectedPath);
    }

    private async Task CopyToClipboard(string text)
    {
        await Clipboard.SetTextAsync(text);
        snackbar.Add($"Copied to clipboard: {text}");
    }
}
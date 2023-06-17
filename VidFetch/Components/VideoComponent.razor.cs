using Microsoft.AspNetCore.Components;
using MudBlazor;
using YoutubeExplode.Videos;

namespace VidFetch.Components;

public partial class VideoComponent
{
    [Parameter]
    public string VideoId { get; set; }

    [Parameter]
    [EditorRequired]
    public string Url { get; set; }

    [Parameter]
    public int CardSize { get; set; } = 12;

    [Parameter]
    [EditorRequired]
    public string Title { get; set; }

    [Parameter]
    [EditorRequired]
    public string Author { get; set; }

    [Parameter]
    [EditorRequired]
    public string AuthorUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public TimeSpan Duration { get; set; }

    [Parameter]
    [EditorRequired]
    public string VideoThumbnail { get; set; }

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
    public string AuthorThumbnailUrl { get; set; }

    [Parameter]
    public int Index { get; set; }

    private Video video;
    private CancellationTokenSource tokenSource;
    private bool isDownloading = false;
    private bool isDownloadSuccessful = false;
    private bool isSaved = false;
    private double progress = 0;

    protected override async Task OnInitializedAsync()
    {
        isSaved = await videoData.VideoExistAsync(Url, VideoId);
        video = await youtube.GetVideoAsync(Url);
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

        await youtube.DownloadVideoAsync(
            Url,
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
            await videoData.SetVideoAsync(Url, VideoId);
            isSaved = true;
        }
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

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref tokenSource);
        isDownloading = false;
        progress = 0;
    }

    private void LoadWatchPage()
    {
        string encodedUrl = Uri.EscapeDataString(Url);
        navManager.NavigateTo($"/Watch/{encodedUrl}");
    }

    private void AddSnackbar()
    {
        snackbar.Add($"Successfully downloaded {Title}", Severity.Normal);
    }
}
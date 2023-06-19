using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;
using YoutubeExplode.Videos;

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
    private CancellationTokenSource tokenSource;
    private bool isDownloading = false;
    private bool isDownloadSuccessful = false;
    private bool isSaved = false;
    private double progress = 0;

    protected override async Task OnInitializedAsync()
    {
        isSaved = await videoData.VideoExistAsync(Video.Url, Video.VideoId);
        video = await youtube.GetVideoAsync(Video.Url);
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadNullData();
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
            Video.Url,
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
            await videoData.SetVideoAsync(Video.Url, Video.VideoId);
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

    private async Task OpenUrl(string text)
    {
        await Launcher.OpenAsync(text);
    }

    private async Task LoadNullData()
    {
        bool isAuthorThumbnailEmpty = string.IsNullOrWhiteSpace(Video.AuthorThumbnailUrl);

        if (isAuthorThumbnailEmpty)
        {
            string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";

            var channel = await youtube.GetChannelAsync(Video.AuthorUrl);
            string channelThumbnail = channel.Thumbnails?.Count > 0 ? channel.Thumbnails[0].Url : defaultUrl;
            Video.AuthorThumbnailUrl =  channelThumbnail;
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
        string encodedUrl = Uri.EscapeDataString(Video.Url);
        navManager.NavigateTo($"/Watch/{encodedUrl}");
    }

    private void AddSnackbar()
    {
        snackbar.Add($"Successfully downloaded {Video.Title}", Severity.Normal);
    }
}
using Microsoft.AspNetCore.Components;
using MudBlazor;
using YoutubeExplode.Playlists;

namespace VidFetch.Components;

public partial class PlaylistVideoComponent
{
    [Parameter]
    [EditorRequired]
    public PlaylistVideo Model { get; set; }

    [Parameter]
    [EditorRequired]
    public string SelectedExtension { get; set; }

    [Parameter]
    [EditorRequired]
    public string SelectedPath { get; set; }

    [Parameter]
    [EditorRequired]
    public int Index { get; set; }

    private bool isDownloading = false;
    private bool isDownloadSuccessful = false;
    private double progress = 0;
    private CancellationTokenSource tokenSource;

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
            Model.Url,
            SelectedPath,
            SelectedExtension,
            progressReporter,
            cancellationToken);

        AddSnackbar();
        CancelVideoDownload();
        isDownloadSuccessful = true;
    }

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref tokenSource);
        isDownloading = false;
        progress = 0;
    }

    private void LoadWatchPage()
    {
        string encodedUrl = Uri.EscapeDataString(Model.Url);
        navManager.NavigateTo($"/Watch/{encodedUrl}");
    }

    private void AddSnackbar()
    {
        snackbar.Add($"Successfully downloaded {Model.Title}", Severity.Normal);
    }

    private async Task OpenFolderLocation()
    {
        await folderHelper.OpenFolderLocationAsync(SelectedPath);
    }
}
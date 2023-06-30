using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexVideo
{
    [Parameter]
    [EditorRequired]
    public EventCallback SwitchEvent { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string> GetPlaylistUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string> AddVideos { get; set; }


    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
    private CancellationTokenSource _allVideosTokenSource;
    private string _videoUrl = "";
    private string _videoSearchText = "";
    private string _currentDownloadingVideo = "";
    private double _videosProgress = 0;

    private async Task LoadVideoOrPlaylistVideos()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_videoUrl))
            {
                return;
            }

            if (IsPlaylistUrl())
            {
                await GetPlaylistUrl.InvokeAsync(_videoUrl);
                await AddVideos.InvokeAsync(_videoUrl);

                await SwitchEvent.InvokeAsync();
            }
            else
            {
                await LoadSingleVideo();
            }

            _videoUrl = "";
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task LoadSingleVideo()
    {
        await OpenLoading.InvokeAsync(true);
        var video = await youtube.GetVideoAsync(_videoUrl);

        if (IsVideoNotLoaded(video.VideoId))
        {
            videoLibrary.Videos.Add(video);
        }

        if (settingsLibrary.SaveVideos)
        {
            await SaveVideos();
        }

        await OpenLoading.InvokeAsync(false);
    }

    private async Task SaveVideos()
    {
        foreach (var v in videoLibrary.Videos)
        {
            await videoData.SetVideoAsync(v.Url, v.VideoId);
        }
    }

    private async Task DownloadAllVideos()
    {
        try
        {
            if (File.Exists(settingsLibrary.FfmpegPath) is false)
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var cancellationToken = tokenHelper.InitializeToken(ref _allVideosTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _videosProgress, value);
            });

            foreach (var v in videoLibrary.Videos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentDownloadingVideo = v.Title;

                await youtube.DownloadVideoAsync(
                    v.Url,
                    progressReport,
                    cancellationToken);

                AddSnackbar(v.Title);
            }

            _currentDownloadingVideo = "";
            CancelVideosDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }


    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Videos, searchInput);
    }

    private void FilterVideos()
    {
        videoLibrary.Videos = searchHelper.FilterList(videoLibrary.Videos, _videoSearchText);
    }

    private void AddSnackbar(string title)
    {
        snackbar.Add($"Successfully downloaded {title}", Severity.Normal);
    }

    private void CancelVideosDownload()
    {
        tokenHelper.CancelRequest(ref _allVideosTokenSource);
        _videosProgress = 0;
        _currentDownloadingVideo = "";
    }

    private void ClearVideos()
    {
        videoLibraryHelper.ClearVideos(ref _videosProgress);
    }

    private void RemoveVideo(VideoModel video)
    {
        videoLibraryHelper.RemoveVideo(video);
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        progressVariable = value;
        StateHasChanged();
    }

    private async Task OpenFileLocation()
    {
        if (string.IsNullOrWhiteSpace(settingsLibrary.SelectedPath))
        {
            return;
        }

        await folderHelper.OpenFolderLocationAsync();
    }

    private string GetDownloadText()
    {
        if (videoLibrary.Videos?.Count <= 0)
        {
            return "Download Video";
        }

        if (videoLibrary.Videos?.Count == 1)
        {
            return "Download 1 Video";
        }

        return $"Download {videoLibrary.Videos?.Count} Videos";
    }

    private string GetSearchBarText()
    {
        if (videoLibrary?.Videos.Count <= 0)
        {
            return "Search Video";
        }

        if (videoLibrary.Videos?.Count == 1)
        {
            return "Search 1 Video";
        }

        return $"Search {videoLibrary.Videos?.Count} Videos";
    }

    private bool IsVideoNotLoaded(string videoId)
    {
        return videoLibrary.Videos.Any(v => v.VideoId == videoId)is false;
    }

    private bool IsPlaylistUrl()
    {
        return Uri.IsWellFormedUriString(_videoUrl, UriKind.Absolute) && _videoUrl.Contains("list=");
    }
}
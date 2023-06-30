using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexPlaylistVideo
{
    [Parameter]
    [EditorRequired]
    public string PlaylistUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
    private CancellationTokenSource _playlistTokenSource;
    private CancellationTokenSource _videoTokenSource;
    private PlaylistModel _playlist = new();
    private string _playlistUrl = "";
    private string _searchText = "";
    private string _currentDownloadingVideo = "";
    private string _firstVideoInPlaylistUrl = "";
    private bool _showDialog = false;
    private double _playlistProgress = 0;
    private double _videoProgress = 0;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(PlaylistUrl) is false)
        {
            _playlist = await youtube.GetPlaylistAsync(PlaylistUrl);
        }
    }

    private async Task LoadPlaylist()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_playlistUrl))
            {
                return;
            }

            if (IsUrlPlaylist())
            {
                await LoadPlaylistVideos();
                _showDialog = true;
                _firstVideoInPlaylistUrl = _playlistUrl;
            }
            else
            {
                snackbar.Add("Please enter a playlist url.");
            }

            _playlistUrl = "";
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task LoadPlaylistVideos()
    {
        await OpenLoading.InvokeAsync(true);
        var videos = await youtube.GetPlayListVideosAsync(_playlistUrl);
        foreach (var v in videos)
        {
            if (IsVideoNotLoaded(v.VideoId))
            {
                videoLibrary.PlaylistVideos.Add(v);
            }
        }

        if (settingsLibrary.SaveVideos)
        {
            await SavePlaylistVideos();
        }

        await OpenLoading.InvokeAsync(false);
    }

    private async Task SavePlaylistVideos()
    {
        foreach (var v in videoLibrary.PlaylistVideos)
        {
            await videoData.SetVideoAsync(v.Url, v.VideoId);
        }
    }

    private async Task DownloadAllPlaylists()
    {
        try
        {
            if (File.Exists(settingsLibrary.FfmpegPath) is false)
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var cancellationToken = tokenHelper.InitializeToken(ref _playlistTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _playlistProgress, value);
            });

            foreach (var v in videoLibrary.PlaylistVideos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentDownloadingVideo = v.Title;


                await youtube.DownloadVideoAsync(
                    v.Url,
                    progressReport,
                    cancellationToken,
                    true,
                    _playlist.Title);

                AddSnackbar(v.Title);
            }

            CancelPlaylistDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task DownloadVideo(string url)
    {
        try
        {
            if (File.Exists(settingsLibrary.FfmpegPath)is false)
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var cancellationToken = tokenHelper.InitializeToken(ref _videoTokenSource);
            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _videoProgress, value);
            });
            await youtube.DownloadVideoAsync(url, progressReport, cancellationToken);
            CancelVideoDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task<IEnumerable<string>> SearchPlaylistVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.PlaylistVideos, searchInput);
    }

    private void FilterPlaylistVideo()
    {
        videoLibrary.PlaylistVideos = searchHelper.FilterList(videoLibrary.PlaylistVideos, _searchText);
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        progressVariable = value;
        StateHasChanged();
    }

    private void CancelPlaylistDownload()
    {
        tokenHelper.CancelRequest(ref _playlistTokenSource);
        _playlistProgress = 0;
        _currentDownloadingVideo = "";
    }

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref _videoTokenSource);
    }

    private void AddSnackbar(string title)
    {
        snackbar.Add($"Successfully downloaded {title}", Severity.Normal);
    }

    private void ClearPlaylistVideos()
    {
        videoLibraryHelper.ClearPlaylistVideos(ref _playlistProgress);
    }

    private void RemovePlaylistVideo(VideoModel video)
    {
        var v = videoLibrary.PlaylistVideos.FirstOrDefault(v => v.VideoId == video.VideoId || v.Url == video.Url);
        videoLibraryHelper.RemovePlaylistVideo(v);
    }

    private bool IsUrlPlaylist()
    {
        return Uri.IsWellFormedUriString(_playlistUrl, UriKind.Absolute) && _playlistUrl.Contains("list=");
    }

    private bool IsVideoNotLoaded(string videoId)
    {
        return videoLibrary.Videos.Any(v => v.VideoId == videoId)is false;
    }

    private int GetIndex(VideoModel playlistVideo)
    {
        int index = videoLibrary.PlaylistVideos.IndexOf(playlistVideo);
        return index + 1;
    }

    private string GetDownloadText()
    {
        if (videoLibrary.PlaylistVideos?.Count <= 0)
        {
            return "Download Video";
        }

        if (videoLibrary.PlaylistVideos?.Count == 1)
        {
            return "Download 1 Video";
        }

        return $"Download {videoLibrary.PlaylistVideos?.Count} Videos";
    }

    private string GetSearchBarText()
    {
        if (videoLibrary?.PlaylistVideos.Count <= 0)
        {
            return "Search Video";
        }

        if (videoLibrary.PlaylistVideos?.Count == 1)
        {
            return "Search 1 Video";
        }

        return $"Search {videoLibrary.PlaylistVideos?.Count} Videos";
    }
}
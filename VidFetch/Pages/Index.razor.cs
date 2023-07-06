using MudBlazor;
using VidFetchLibrary.Language;

namespace VidFetch.Pages;

public partial class Index
{
    private const string VideoId = "Videos";
    private const string PlaylistVideoId = "Playlists-Videos";
    private const string ChannelId = "Channels";
    private const string PlaylistId = "Playlists";
    private string _playlistUrl = "";
    private bool _isVisible = false;
    private MudTabs _tabs;

    private async Task LoadPlaylistVideos(string url)
    {
        ToggleLoadingOverlay(true);
        var videos = await youtube.GetPlayListVideosAsync(url);

        foreach (var v in videos)
        {
            videoLibrary.PlaylistVideos.Add(v);
        }

        ToggleLoadingOverlay(false);
        StateHasChanged();
    }

    private void Activate(object id)
    {
        _tabs.ActivatePanel(id);
    }

    private void ToggleLoadingOverlay(bool isVisible)
    {
        _isVisible = isVisible;
    }

    private void GetPlaylistUrl(string url)
    {
        _playlistUrl = url;
    }

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = languageExtension.GetDictionary();
        return dictionary;
    }
}
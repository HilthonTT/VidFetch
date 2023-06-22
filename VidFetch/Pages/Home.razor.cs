using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class Home
{
    private List<VideoModel> _savedVideos;
    private List<ChannelModel> _savedChannels;
    protected override async Task OnInitializedAsync()
    {
        _savedVideos = await videoData.GetAllVideosAsync();
        _savedChannels = await channelData.GetAllChannelsAsync();
    }

    private void LoadDownloadPage()
    {
        navManager.NavigateTo("/Download");
    }

    private void LoadSavedMediaPage()
    {
        navManager.NavigateTo("/SavedMedias");
    }

    private void LoadSearchPage()
    {
        navManager.NavigateTo("/Search");
    }

    private void LoadSettingsPage()
    {
        navManager.NavigateTo("/Settings");
    }

    private void LoadVideoPage(VideoModel video)
    {
        string encodedUrl = Uri.EscapeDataString(video.Url);
        navManager.NavigateTo($"/Watch/{encodedUrl}");
    }

    private void LoadChannelPage(ChannelModel channel)
    {
        string encodedUrl = Uri.EscapeDataString(channel.Url);
        navManager.NavigateTo($"/Channel/{encodedUrl}");
    }
}
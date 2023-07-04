using MudBlazor;
using VidFetchLibrary.Models;
using Color = MudBlazor.Color;

namespace VidFetch.Pages;

public partial class Home
{
    private const int MaxItem = 10;
    private List<VideoModel> _savedVideos;
    private List<ChannelModel> _savedChannels;

    protected override async Task OnInitializedAsync()
    {
        await LoadVideos();
        await LoadChannels();
    }

    private async Task LoadVideos()
    {
        _savedVideos = await videoData.GetAllVideosAsync();

        _savedVideos = _savedVideos
            .Take(MaxItem)
            .ToList();
    }

    private async Task LoadChannels()
    {
        _savedChannels = await channelData.GetAllChannelsAsync();

        _savedChannels = _savedChannels
            .Take(MaxItem)
            .ToList();
    }

    private void LoadPasteLinkPage()
    {
        navManager.NavigateTo("/PasteLinkPage");
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

    private static string GetIcon(bool selected)
    {
        if (selected)
        {
            return Icons.Material.Filled.CheckCircle;
        }
        else
        {
            return Icons.Material.Filled.Circle;
        }
    }

    private static Color GetIconColor(bool selected)
    {
        if (selected)
        {
            return Color.Success;
        }
        else
        {
            return Color.Inherit;
        }
    }
}
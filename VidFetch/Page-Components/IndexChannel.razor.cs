using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexChannel
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private List<ChannelModel> _visibleChannels = new();
    private string _channelUrl = "";
    private string _channelSearchText = "";
    private int _loadedItems = 6;

    protected override void OnInitialized()
    {
        _visibleChannels = videoLibrary.Channels.Take(_loadedItems).ToList();
    }

    private void LoadMoreChannels()
    {
        int itemsPerPage = 6;
        int channelsCount = videoLibrary.Channels.Count;

        _loadedItems += itemsPerPage;

        if (_loadedItems > channelsCount)
        {
            _loadedItems = channelsCount;
        }

        _visibleChannels = videoLibrary.Channels.Take(_loadedItems).ToList();
    }

    private async Task LoadChannel()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_channelUrl))
            {
                return;
            }

            if (Uri.IsWellFormedUriString(_channelUrl, UriKind.Absolute))
            {
                await OpenLoading.InvokeAsync(true);

                var channel = await youtube.GetChannelAsync(_channelUrl);

                if (IsChannelNull(channel))
                {
                    videoLibrary.Channels.Add(channel);
                }

                await OpenLoading.InvokeAsync(false);
            }
            else
            {
                snackbar.Add("Invalid Url", Severity.Warning);
            }

            _channelUrl = "";
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task<IEnumerable<string>> SearchChannels(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Channels, searchInput);
    }

    private void FilterChannels()
    {
        videoLibrary.Channels = searchHelper.FilterList(videoLibrary.Channels, _channelSearchText);
    }

    private void ClearChannels()
    {
        videoLibraryHelper.ClearChannels();
    }

    private void RemoveChannel(ChannelModel channel)
    {
        var c = videoLibrary.Channels.FirstOrDefault(c => c.ChannelId == channel.ChannelId || c.Url == channel.Url);
        videoLibraryHelper.RemoveChannel(c);
    }

    private string GetChannelSearchBarText()
    {
        if (videoLibrary?.Channels.Count <= 0)
        {
            return "Search Channel";
        }

        if (videoLibrary.Channels?.Count == 1)
        {
            return "Search 1 Channel";
        }

        return $"Search {videoLibrary.Channels?.Count} Channels";
    }

    private bool IsChannelNull(ChannelModel channel)
    {
        return videoLibrary.Channels.FirstOrDefault(c => c.ChannelId == channel.ChannelId) is null;
    }
}
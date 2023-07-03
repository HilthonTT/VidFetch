using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SavedMediaChannel
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private List<ChannelModel> _channels = new();
    private List<ChannelModel> _visibleChannels = new();
    private string _searchText = "";
    private int _loadedItems = 6;

    protected override async Task OnInitializedAsync()
    {
        await LoadChannels();
    }

    private void LoadMoreChannels()
    {
        int itemsPerPage = 6;
        int channelCount = _channels.Count;
        _loadedItems += itemsPerPage;
        if (_loadedItems > channelCount)
        {
            _loadedItems = channelCount;
        }

        _visibleChannels = _channels.Take(_loadedItems).ToList();
    }

    private async Task LoadChannels()
    {
        await OpenLoading.InvokeAsync(true);
        _channels = await channelData.GetAllChannelsAsync();
        _visibleChannels = _channels.Take(_loadedItems).ToList();
        await OpenLoading.InvokeAsync(false);
    }

    private async Task DeleteChannel(ChannelModel channel)
    {
        _channels.Remove(channel);
        _visibleChannels.Remove(channel);
        await channelData.DeleteChannelAsync(channel);
    }

    private async Task<IEnumerable<string>> SearchChannels(string searchInput)
    {
        return await searchHelper.SearchAsync(_channels, searchInput);
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterChannels();
    }

    private void FilterChannels()
    {
        _channels = searchHelper
            .FilterList(_channels, _searchText);

        _visibleChannels = searchHelper
            .FilterList(_channels, _searchText)
            .Take(_loadedItems)
            .ToList();
    }

    private string GetSearchBarText()
    {
        if (_channels?.Count <= 0)
        {
            return "Search Channel";
        }

        if (_channels?.Count == 1)
        {
            return "Search 1 Channel";
        }

        return $"Search {_channels?.Count} Channels";
    }
}
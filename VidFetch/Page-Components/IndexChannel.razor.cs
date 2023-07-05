using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexChannel
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private const string PageName = nameof(IndexChannel);
    private const int ItemsPerPage = 6;

    private CancellationTokenSource _tokenSource;
    private List<ChannelModel> _visibleChannels = new();
    private string _channelUrl = "";
    private string _searchText = "";
    private int _loadedItems = 6;

    protected override void OnInitialized()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(PageName, ItemsPerPage);
        _visibleChannels = videoLibrary.Channels.Take(_loadedItems).ToList();
    }

    private void LoadMoreChannels()
    {
        int channelsCount = videoLibrary.Channels.Count;

        _loadedItems += ItemsPerPage;

        if (_loadedItems > channelsCount)
        {
            _loadedItems = channelsCount;
        }

        _visibleChannels = videoLibrary.Channels.Take(_loadedItems).ToList();
        loadedItemsCache.SetLoadedItemsCount(PageName, _loadedItems);
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

                    _visibleChannels = videoLibrary.Channels.Take(_loadedItems).ToList();
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

    private async Task SaveAllChannels()
    {
        try
        {
            var channelsCopy = videoLibrary.Channels.ToList();
            var token = tokenHelper.InitializeToken(ref _tokenSource);

            foreach (var c in channelsCopy)
            {
                token.ThrowIfCancellationRequested();
                await channelData.SetChannelAsync(c.Url, c.ChannelId);
            }

            CancelSaveChannel();
            snackbar.Add("Successfully saved all channels");
        }
        catch
        {
            snackbar.Add("An error occured while saving all channels", Severity.Error);
        }
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterChannels();
    }

    private void FilterChannels()
    {
        videoLibrary.Channels = searchHelper
            .FilterList(videoLibrary.Channels, _searchText);

        _visibleChannels = searchHelper
            .FilterList(videoLibrary.Channels, _searchText)
            .Take(_loadedItems)
            .ToList();
    }

    private void ClearChannels()
    {
        videoLibraryHelper.ClearChannels();
        _visibleChannels.Clear();
    }

    private void RemoveChannel(ChannelModel channel)
    {
        videoLibraryHelper.RemoveChannel(channel);
        _visibleChannels.Remove(channel);
    }

    private void CancelSaveChannel()
    {
        tokenHelper.CancelRequest(ref  _tokenSource);
    }

    private string GetSearchBarText()
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
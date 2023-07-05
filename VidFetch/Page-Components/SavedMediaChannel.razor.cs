using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SavedMediaChannel
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private const string PageName = nameof(SavedMediaChannel);
    private const int ItemsPerPage = 6;

    private CancellationTokenSource _tokenSource;
    private List<ChannelModel> _channels = new();
    private List<ChannelModel> _visibleChannels = new();
    private string _searchText = "";
    private int _loadedItems = 6;
    private bool _isVisible = false;

    protected override async Task OnInitializedAsync()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(PageName, ItemsPerPage);
        await LoadChannels();
    }

    private void LoadMoreChannels()
    {
        int channelCount = _channels.Count;
        _loadedItems += ItemsPerPage;

        if (_loadedItems > channelCount)
        {
            _loadedItems = channelCount;
        }

        _visibleChannels = _channels.Take(_loadedItems).ToList();
        loadedItemsCache.SetLoadedItemsCount(PageName, _loadedItems);
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

    private async Task UpdateAllChannels()
    {
        try
        {
            var channelsCopy = _channels.ToList();
            var token = tokenHelper.InitializeToken(ref _tokenSource);

            foreach (var c in channelsCopy)
            {
                token.ThrowIfCancellationRequested();
                await UpdateChannel(c, token);
            }

            CancelUpdateChannel();
            snackbar.Add("Successfully updated.");
        }
        catch (OperationCanceledException)
        {
            snackbar.Add("Update canceled.", Severity.Error);
        }
        catch
        {
            snackbar.Add("An error occured while updating.", Severity.Error);
        }
    }

    private async Task UpdateChannel(ChannelModel channel, CancellationToken token)
    {
        var newChannel = await youtube.GetChannelAsync(channel.Url, token);

        if (newChannel is null)
        {
            RemoveChannel(channel);
            snackbar.Add($"{channel.Title} no longer exists. It has been deleted", Severity.Error);
            await channelData.DeleteChannelAsync(channel);
        }
        else
        {
            await channelData.SetChannelAsync(channel.Url, channel.ChannelId);
        }
    }

    private async Task DeleteAllChannels()
    {
        CloseDialog();

        var channelCopy = _channels.ToList();

        foreach (var c in channelCopy)
        {
            RemoveChannel(c);
            await channelData.DeleteChannelAsync(c);
        }
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

    private void CancelUpdateChannel()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
    }

    private void RemoveChannel(ChannelModel channel)
    {
        _visibleChannels?.Remove(channel);
        _channels?.Remove(channel);
    }

    private void OpenDialog()
    {
        _isVisible = true;
    }

    private void CloseDialog()
    {
        _isVisible = false;
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
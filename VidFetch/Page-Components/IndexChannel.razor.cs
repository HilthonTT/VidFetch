using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexChannel
{
    private string _channelUrl = "";
    private string _channelSearchText = "";
    private bool _isChannelLoading = false;
    private async Task LoadChannel()
    {
        try
        {;
            if (string.IsNullOrWhiteSpace(_channelUrl))
            {
                return;
            }

            if (Uri.IsWellFormedUriString(_channelUrl, UriKind.Absolute))
            {
                _isChannelLoading = true;
                var channel = await youtube.GetChannelAsync(_channelUrl);
                bool channelIsNull = videoLibrary.Channels.FirstOrDefault(c => c.ChannelId == channel.ChannelId)is null;
                if (channelIsNull)
                {
                    videoLibrary.Channels.Add(channel);
                }

                _isChannelLoading = false;
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
            _isChannelLoading = false;
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
}
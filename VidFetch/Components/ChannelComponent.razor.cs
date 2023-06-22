using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Models;

namespace VidFetch.Components;

public partial class ChannelComponent
{
    [Parameter]
    [EditorRequired]
    public ChannelModel Channel { get; set; }

    [Parameter]
    public int CardSize { get; set; } = 12;

    [Parameter]
    [EditorRequired]
    public EventCallback<ChannelModel> RemoveEvent { get; set; }

    private bool _isSaved = false;
    protected override async Task OnInitializedAsync()
    {
        _isSaved = await channelData.ChannelExistsAsync(Channel.Url, Channel.ChannelId);
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadNullData();
    }

    private async Task SaveChannel()
    {
        if (_isSaved is false)
        {
            await channelData.SetChannelAsync(Channel.Url, Channel.ChannelId);
            snackbar.Add($"Successfully saved {Channel.Title}");
            _isSaved = true;
        }
    }

    private async Task LoadNullData()
    {
        bool isThumbnailEmpty = string.IsNullOrWhiteSpace(Channel.ThumbnailUrl);
        if (isThumbnailEmpty)
        {
            string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";
            var channel = await youtube.GetChannelAsync(Channel.Url);
            string channelThumbnail = string.IsNullOrWhiteSpace(channel.ThumbnailUrl) ? defaultUrl : channel.ThumbnailUrl;
            Channel.ThumbnailUrl = channelThumbnail;
        }
    }

    private async Task Remove()
    {
        await RemoveEvent.InvokeAsync(Channel);
    }

    private static async Task OpenUrl(string text)
    {
        await Launcher.OpenAsync(text);
    }

    private void LoadChannelPage()
    {
        string encodedUrl = Uri.EscapeDataString(Channel.Url);
        navManager.NavigateTo($"/Channel/{encodedUrl}");
    }
}
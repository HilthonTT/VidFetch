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
    public string SelectedExtension { get; set; }

    [Parameter]
    [EditorRequired]
    public string SelectedPath { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<ChannelModel> RemoveEvent { get; set; }

    private bool isSaved = false;
    protected override async Task OnInitializedAsync()
    {
        isSaved = await channelData.ChannelExistsAsync(Channel.Url, Channel.ChannelId);
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadNullData();
    }

    private async Task SaveChannel()
    {
        if (isSaved is false)
        {
            await channelData.SetChannelAsync(Channel.Url, Channel.ChannelId);
            snackbar.Add($"Successfully saved {Channel.Title}");
            isSaved = true;
        }
    }

    private async Task LoadNullData()
    {
        bool isThumbnailEmpty = string.IsNullOrWhiteSpace(Channel.ThumbnailUrl);
        if (isThumbnailEmpty)
        {
            string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";
            var channel = await youtube.GetChannelAsync(Channel.Url);
            string channelThumbnail = channel.Thumbnails?.Count > 0 ? channel.Thumbnails[0].Url : defaultUrl;
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
using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Language;
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadChannelData();
        }
    }

    private async Task LoadChannelData()
    {
        var playlistTasks = new List<Task>()
        {
            LoadThumbnail(),
        };

        await Task.WhenAll(playlistTasks);
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

    private async Task LoadThumbnail()
    {
        bool isThumbnailEmpty = string.IsNullOrWhiteSpace(Channel.ThumbnailUrl);

        if (isThumbnailEmpty)
        {
            var channel = await youtube.GetChannelAsync(Channel.Url);
            string channelThumbnail = string.IsNullOrWhiteSpace(channel.ThumbnailUrl) ? "" : channel.ThumbnailUrl;
            Channel.ThumbnailUrl = channelThumbnail;

            StateHasChanged();
        }
    }

    private async Task Remove()
    {
        await RemoveEvent.InvokeAsync(Channel);
    }

    private async Task OpenUrl(string text)
    {
        await launcher.OpenAsync(text);
    }

    private void LoadChannelPage()
    {
        string encodedUrl = Uri.EscapeDataString(Channel.Url);
        navManager.NavigateTo($"/Channel/{encodedUrl}");
    }

    private string SaveChannelText()
    {
        string channelText = GetDictionary()[KeyWords.Channel];
        string saveText = GetDictionary()[KeyWords.Save];

        return $"{saveText} {channelText}";
    }

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = languageExtension.GetDictionary();
        return dictionary;
    }
}
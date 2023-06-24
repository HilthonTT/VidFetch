using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class Channel
{
    [Parameter]
    public string Url { get; set; }

    private const int MaxVideoCount = 50;
    private ChannelModel _channel;
    private List<VideoModel> _videos = new();
    private bool _isSaved = false;
    private string _channelId = "";
    protected override async Task OnInitializedAsync()
    {
        _channelId = UrlRegex().Match(Url).Value;
        await LoadData();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadNullData();
    }

    private async Task LoadNullData()
    {
        bool isThumbnailEmpty = _channel is not null && string.IsNullOrWhiteSpace(_channel?.ThumbnailUrl);
        if (isThumbnailEmpty)
        {
            string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";
            var c = await youtube.GetChannelAsync(_channel.Url);
            string channelThumbnail = string.IsNullOrWhiteSpace(c.ThumbnailUrl) ? defaultUrl : _channel.ThumbnailUrl;
            _channel.ThumbnailUrl = channelThumbnail;
        }
    }

    private async Task SaveChannel()
    {
        if (_isSaved is false)
        {
            await channelData.SetChannelAsync(_channel.Url, _channel.ChannelId);
            snackbar.Add($"Successfully saved {_channel.Title}");
            _isSaved = true;
        }
    }

    private async Task DeleteChannel()
    {
        if (_isSaved)
        {
            await channelData.DeleteChannelAsync(_channel);
            snackbar.Add($"Successfully deleted {_channel.Title}");
            _isSaved = false;
        }
    }

    private async Task OpenUrl()
    {
        await launcher.OpenAsync(Url);
    }

    private async Task LoadData()
    {
        bool isUrlValid = Uri.IsWellFormedUriString(Url, UriKind.Absolute);
        if (isUrlValid)
        {
            _channel = await channelData.GetChannelAsync(Url, _channelId);
            if (_channel is null)
            {
                _channel = await youtube.GetChannelAsync(Url);
            }

            _videos = await youtube.GetChannelVideosAsync(Url);
            _isSaved = await channelData.ChannelExistsAsync(Url, _channelId);
        }
    }

    private void RemoveVideo(VideoModel video)
    {
        _videos.Remove(video);
    }

    [GeneratedRegex("(?<=channel\\/)([\\w-]+)")]
    private static partial Regex UrlRegex();
}
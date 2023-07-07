using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using VidFetchLibrary.Language;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class Channel
{
    [Parameter]
    public string Url { get; set; }

    private ChannelModel _channel;
    private List<VideoModel> _videos = new();
    private List<VideoModel> _visibleVideos = new();
    private bool _isSaved = false;
    private string _channelId = "";
    private int _loadedItems = 6;

    protected override async Task OnInitializedAsync()
    {
        _channelId = UrlRegex().Match(Url).Value;
        await LoadData();

        _visibleVideos = _videos.Take(_loadedItems).ToList();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadNullData();
    }

    private void LoadMoreVideos()
    {
        int itemsPerPage = 6;
        int videosCount = _videos.Count;

        _loadedItems += itemsPerPage;

        if (_loadedItems > videosCount)
        {
            _loadedItems = videosCount;
        }

        _visibleVideos = _videos.Take(_loadedItems).ToList();
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

            string deleteMessage;
            snackbar.Add($"Successfully deleted {_channel.Title}");
            _isSaved = false;

            navManager.NavigateTo("/SavedMedias");
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

    private bool IsChannelNull()
    {
        if (_channel is null)
        {
            return true;
        }

        return false;
    }

    private void RemoveVideo(VideoModel video)
    {
        _visibleVideos.Remove(video);
    }

    private Dictionary<KeyWords, string> GetDictionary(string text = "")
    {
        var dictionary = languageExtension.GetDictionary(text);
        return dictionary;
    }

    private string GetVideoCount()
    {
        string videoCount = _videos?.Count.ToString();
        string videoText = GetDictionary(videoCount)[KeyWords.ChannelVideoCount];

        return videoText;
    }

    [GeneratedRegex("(?<=channel\\/)([\\w-]+)")]
    private static partial Regex UrlRegex();
}
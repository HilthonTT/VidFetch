using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class Watch
{
    [Parameter]
    public string Url { get; set; }

    private string _sourcePath = "";
    private VideoModel _video;
    private ChannelModel _channel;

    protected override async Task OnInitializedAsync()
    {
        await LoadVideo();
        _sourcePath = $"https://www.youtube.com/embed/{_video.VideoId}";
        if (_video is not null)
        {
            _channel = await youtube.GetChannelAsync(_video.AuthorUrl);
        }
    }

    private async Task OpenUrl(string text)
    {
        await launcher.OpenAsync(text);
    }

    private async Task LoadVideo()
    {
        string channelIdentifier = ChannelIdRegex().Match(Url).Value;

        _video = await videoData.GetVideoAsync(Url, channelIdentifier);

        if (_video is null)
        {
            _video = await youtube.GetVideoAsync(Url);
        }
    }

    private string FormatDescription()
    {
        if (_video is not null)
        {
            string description = MyRegex().Replace(_video.Description, "<a href=\"$1\">$1</a>");

            // Split the description into separate lines
            string[] lines = description.Split('\n');

            // Add bullet points to each line
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = "• " + lines[i].Trim();
            }

            // Join the lines back together with line breaks
            string formattedDescription = string.Join("<br>", lines);
            return formattedDescription;
        }

        return "";
    }

    [GeneratedRegex("(https?://[^\\s]+)")]
    private static partial Regex MyRegex();

    [GeneratedRegex("(?<=channel\\/)([\\w-]+)")]
    private static partial Regex ChannelIdRegex();
}
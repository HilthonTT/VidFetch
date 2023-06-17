using Microsoft.AspNetCore.Components;
using YoutubeExplode.Videos;
using System.Text.RegularExpressions;
using YoutubeExplode.Channels;

namespace VidFetch.Pages;

public partial class Watch
{
    [Parameter]
    public string Url { get; set; }
    public string SourcePath { get; set; }
    public Video Video { get; set; }

    private Channel channel;

    protected override async Task OnInitializedAsync()
    {
        Video = await youtube.GetVideoAsync(Url);
        SourcePath = $"https://www.youtube.com/embed/{Video.Id}";
        if (Video is not null)
        {
            channel = await youtube.GetChannelAsync(Video.Author.ChannelUrl);
        }
    }

    private async Task CopyToClipboard(string text)
    {
        await Clipboard.SetTextAsync(text);
        snackbar.Add($"Copied to clipboard: {text}");
    }

    private string FormatDescription()
    {
        if (Video is not null)
        {
            string description = MyRegex().Replace(Video.Description, "<a href=\"$1\">$1</a>");

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
}
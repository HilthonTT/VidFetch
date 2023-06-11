using Microsoft.AspNetCore.Components;
using YoutubeExplode.Videos;
using System.Text.RegularExpressions;

namespace VidFetch.Pages;

public partial class Watch
{
    [Parameter]
    public string Url { get; set; }
    public string SourcePath { get; set; }
    public Video Video { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Video = await youtubeDownloader.GetVideoAsync(Url);
        SourcePath = $"https://www.youtube.com/embed/{Video.Id}";
    }

    private string FormatDescription()
    {
        if (Video is not null)
        {
            string description = "";
            description = MyRegex().Replace(Video.Description, "<a href=\"$1\">$1</a>");

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
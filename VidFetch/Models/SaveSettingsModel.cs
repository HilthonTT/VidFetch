using System.ComponentModel.DataAnnotations;

namespace VidFetch.Models;
public class SaveSettingsModel
{
    [Display(Name = "Dark Mode")]
    [Required(ErrorMessage = "You must set your dark mode settings.")]
    public bool IsDarkMode { get; set; } = true;

    [Display(Name = "Download Subtitles")]
    [Required(ErrorMessage = "You must set your set your download subtitles settings.")]
    public bool DownloadSubtitles { get; set; } = false;


    [Display(Name = "Save Videos")]
    [Required(ErrorMessage = "You must set your set your save videos settings.")]
    public bool SaveVideos { get; set; } = false;
}

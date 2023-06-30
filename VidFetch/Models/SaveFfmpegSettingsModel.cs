using System.ComponentModel.DataAnnotations;
using VidFetchLibrary.Data;

namespace VidFetch.Models;
public class SaveFfmpegSettingsModel
{
    [Display(Name = "Selected Resolution")]
    [Required(ErrorMessage = "You must set your selected resolution settings")]
    public VideoResolution SelectedResolution { get; set; }

    [Display(Name = "Ffmpeg Path")]
    [Required(ErrorMessage = "You must input your Ffmpeg path.")]
    public string FfmpegPath { get; set; }
}

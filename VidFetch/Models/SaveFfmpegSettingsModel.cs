using System.ComponentModel.DataAnnotations;
using VidFetchLibrary.Data;
using VidFetchLibrary.Library;

namespace VidFetch.Models;
public class SaveFfmpegSettingsModel
{
    [Display(Name = "Selected Resolution")]
    [Required(ErrorMessage = "You must set your selected resolution settings")]
    public VideoResolution SelectedResolution { get; set; } = VideoResolution.P1080;

    [Display(Name = "Ffmpeg Path")]
    [Required(ErrorMessage = "You must input your Ffmpeg path.")]
    public string FfmpegPath { get; set; } = "";

    public SaveFfmpegSettingsModel()
    {
        
    }

    public SaveFfmpegSettingsModel(SettingsLibrary settings)
    {
        SelectedResolution = settings.SelectedResolution;
        FfmpegPath = settings.FfmpegPath;
    }
}

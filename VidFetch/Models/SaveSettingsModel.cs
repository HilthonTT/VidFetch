﻿using System.ComponentModel.DataAnnotations;
using VidFetchLibrary.Data;

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

    [Display(Name = "Create Subdirectory Playlist")]
    [Required(ErrorMessage = "You must set your set your create subdirectory playlist.")]
    public bool CreateSubDirectoryPlaylist { get; set; } = true;

    [Display(Name = "Remove After Download")]
    [Required(ErrorMessage = "You must set your set your remove after download settings.")]
    public bool RemoveAfterDownload { get; set; } = false;

    [Display(Name = "Selected Path")]
    [Required(ErrorMessage = "You must set your selected Path settings.")]
    public DownloadPath SelectedPath { get; set; }

    [Display(Name = "Selected Format")]
    [Required(ErrorMessage = "You must set your selected format settings.")]
    public VideoExtension SelectedFormat { get; set; }

    [Display(Name = "Selected Resolution")]
    [Required(ErrorMessage = "You must set your selected resolution settings")]
    public VideoResolution SelectedResolution { get; set; }
}

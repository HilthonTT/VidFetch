﻿using MudBlazor;
using VidFetchLibrary.Language;

namespace VidFetch.Helpers;
public class SnackbarHelper : ISnackbarHelper
{
    private readonly ISnackbar _snackbar;
    private readonly ILanguageExtension _languageExtension;

    public SnackbarHelper(ISnackbar snackbar,
                          ILanguageExtension languageExtension)
    {
        _snackbar = snackbar;
        _languageExtension = languageExtension;
    }

    public void ShowErrorDownloadMessage()
    {
        string message = GetDictionary()
            [KeyWords.DownloadingErrorMessage];

        _snackbar.Add(message, Severity.Error);
    }

    public void ShowSuccessfullyUpdatedDataMessage(string text)
    {
        string message = GetDictionary(text)
            [KeyWords.SuccessfullyUpdatedData];

        _snackbar.Add(message);
    }

    public void ShowErrorWhileUpdatingMessage()
    {
        string message = GetDictionary()
            [KeyWords.ErrorWhileUpdating];

        _snackbar.Add(message, Severity.Error);
    }

    public void ShowSuccessfullySavedVideosMessage()
    {

        string message = GetDictionary()
            [KeyWords.SuccessfullySavedAllVideos];

        _snackbar.Add(message);

    }

    public void ShowErrorWhileSavingMessage()
    {
        string message = GetDictionary()
            [KeyWords.ErrorWhileSaving];

        _snackbar.Add(message, Severity.Error);
    }

    public void ShowNoLongerExistsMessage()
    {
        string message = GetDictionary()
            [KeyWords.NoLongerExistsDelete];

        _snackbar.Add(message, Severity.Error);
    }

    public void ShowErrorOperationCanceledMessage()
    {
        string message = GetDictionary()
            [KeyWords.OperationCanceled];

        _snackbar.Add(message, Severity.Error);

    }

    public void ShowErrorWhileLoadingMessage()
    {
        string message = GetDictionary()
            [KeyWords.ErrorWhileLoadingData];

        _snackbar.Add(message, Severity.Error);
    }

    public void ShowSuccessfullyDownloadedMessage(string text)
    {
        string message = GetDictionary(text)
            [KeyWords.SuccessfullyDownloaded];

        _snackbar.Add(message);
    }

    public void ShowSuccessfullySavedMessage(string text)
    {
        string message = GetDictionary(text)
           [KeyWords.SuccessfullySavedData];

        _snackbar.Add(message);
    }

    public void ShowNoVideoErrorMessage()
    {
        string errorMessage = GetDictionary()
            [KeyWords.NoVideoErrorMessage];

        _snackbar.Add(errorMessage, Severity.Error);
    }

    public void ShowFfmpegError()
    {
        string errorMessage = GetDictionary()
            [KeyWords.FfmpegErrorMessage];

        _snackbar.Add(errorMessage, Severity.Warning);
    }

    public void ShowErrorLoadingPlaylist()
    {
        string errorMessage = GetDictionary()
            [KeyWords.ErrorWhileLoadingPlaylist];

        _snackbar.Add(errorMessage, Severity.Error);
    }

    public void ShowEnterPlaylistUrl()
    {
        string message = GetDictionary()
            [KeyWords.PleaseEnterAPlaylistUrl];

        _snackbar.Add(message);
    }

    public void ShowCurrentlyDownloading(string text)
    {
        string message = GetDictionary(text)
            [KeyWords.CurrentlyDownloading];

        _snackbar.Add(message);
    }

    public void ShowSuccessfullyDeleteMessage(string text)
    {
        string message = GetDictionary(text)
           [KeyWords.SuccessfullyDeletedData];

        _snackbar.Add(message);
    }

    private Dictionary<KeyWords, string> GetDictionary(string text = "")
    {
        var dictionary = _languageExtension.GetDictionary(text);
        return dictionary;
    }
}

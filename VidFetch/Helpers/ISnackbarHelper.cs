namespace VidFetch.Helpers;

public interface ISnackbarHelper
{
    void ShowErrorDownloadMessage();
    void ShowErrorWhileLoadingMessage();
    void ShowErrorWhileSavingMessage();
    void ShowErrorWhileUpdatingMessage();
    void ShowNoLongerExistsMessage();
    void ShowErrorOperationCanceledMessage();
    void ShowSuccessfullyDownloadedMessage();
    void ShowSuccessfullyDownloadedMessage(string text);
    void ShowSuccessfullySavedVideosMessage();
    void ShowSuccessfullyUpdatedDataMessage();
    void ShowEnterPlaylistUrl();
    void ShowErrorLoadingPlaylist();
    void ShowFfmpegError();
    void ShowNoVideoErrorMessage();
    void ShowSuccessfullySavedMessage(string text);
}
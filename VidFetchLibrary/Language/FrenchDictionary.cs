using Microsoft.Extensions.Caching.Memory;

namespace VidFetchLibrary.Language;
public class FrenchDictionary : IFrenchDictionary
{
    private const string CacheName = nameof(FrenchDictionary);
    private const int CacheTime = 5;
    private readonly IMemoryCache _cache;

    public FrenchDictionary(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Dictionary<KeyWords, string> GetDictionary(string text)
    {
        string key = $"{CacheName}-{text}";

        var dictionary = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            return CreateDictionary(text);
        });

        if (dictionary is null)
        {
            _cache.Remove(key);
        }

        return dictionary;
    }

    private static Dictionary<KeyWords, string> CreateDictionary(string text)
    {
        var frenchDictionary = new Dictionary<KeyWords, string>
        {
            { KeyWords.PasteALink, "Coller un lien" },
            { KeyWords.SavedMedias, "Médias enregistrés" },
            { KeyWords.SearchMedias , "Rechercher des médias" },
            { KeyWords.Settings, "Paramètres" },
            { KeyWords.Uploaded, "Téléchargés" },
            { KeyWords.Delete , "Supprimer" },
            { KeyWords.Save, "Enregistrer" },
            { KeyWords.Videos, "Vidéos" },
            { KeyWords.Channels, "Chaînes" },
            { KeyWords.Playlists, "Listes de lecture" },
            { KeyWords.PlaylistVideos, "Vidéos de la liste de lecture" },
            { KeyWords.EnterUrlTextVideo, "Entrez l'URL de la vidéo" },
            { KeyWords.EnterUrlTextChannel, "Entrez l'URL de la chaîne" },
            { KeyWords.EnterUrlTextPlaylist, "Entrez l'URL de la liste de lecture" },
            { KeyWords.SearchVideo, "Rechercher des vidéos" },
            { KeyWords.SearchChannel, "Rechercher des chaînes" },
            { KeyWords.SearchPlaylist, "Rechercher des listes de lecture" },
            { KeyWords.SearchVideoPlural, $"Rechercher {text} vidéos" },
            { KeyWords.SearchChannelPlural, $"Rechercher {text} chaînes" },
            { KeyWords.SearchPlaylistPlural, $"Rechercher {text} listes de lecture" },
            { KeyWords.PlaylistWarning, "Attention : Affiche seulement 200 vidéos pour des raisons de performance." },
            { KeyWords.LoadMore, "Charger plus" },
            { KeyWords.Search, "Rechercher" },
            { KeyWords.AppSettings, "Paramètres de l'application" },
            { KeyWords.DarkMode, "Mode sombre" },
            { KeyWords.DownloadSubtitles, "Télécharger les sous-titres inclus avec la vidéo" },
            { KeyWords.AutoSaveVideo, "Enregistrer automatiquement les vidéos" },
            { KeyWords.CreateSubdirectory, "Créer un sous-dossier pour la playlist" },
            { KeyWords.RemoveVideoDownload, "Supprimer la vidéo après le téléchargement" },
            { KeyWords.SelectedPath, "Choisissez votre dossier de téléchargement préféré." },
            { KeyWords.SelectedFormat, "Choisissez votre format préféré." },
            { KeyWords.SelectedResolution, "Choisissez votre résolution préférée." },
            { KeyWords.SelectedLanguage, "Choisissez votre language préférée." },
            { KeyWords.SearchLabelText, $"Recherchez votre {text}" },
            { KeyWords.SearchHelperText, $"Entrez l'URL ou le titre du {text}" },
            { KeyWords.CancelSearch, "Annuler la recherche" },
            { KeyWords.OpenFolderLocation, "Ouvrir le dossier" },
            { KeyWords.Clear, "Effacer" },
            { KeyWords.Video, "Vidéo" },
            { KeyWords.Channel, "Chaîne" },
            { KeyWords.Playlist, "List de lecture" },
            { KeyWords.Download, "Télécharger" },
            { KeyWords.Cancel, "Annuler" },
            { KeyWords.UpdateData, "Mettre à jour les données" },
            { KeyWords.CancelUpdateData, "Annuler la mise à jour des données" },
            { KeyWords.DeleteAllWarningTitle, "Tout supprimer" },
            { KeyWords.DeleteAllWarningText, $"Supprimer tous vos {text} est irréversible !" },
            { KeyWords.Confirm, "Confirmer" },
            { KeyWords.UrlPlaylistTitle, "Il semble que votre URL soit une playlist." },
            { KeyWords.UrlPlaylistText, "Souhaitez-vous télécharger votre vidéo à partir de cette URL ?" },
            { KeyWords.Remove, "Supprimer" },
            { KeyWords.OpenDetails, "Ouvrir les détails" },
            { KeyWords.OpenUrl, "Ouvrir l'URL" },
            { KeyWords.Watch, "Regarder" },
            { KeyWords.SuccessfullySettings, "Paramètres enregistrés avec succès." },
            { KeyWords.FailedSettings, "Échec de l'enregistrement des paramètres." },
            { KeyWords.FfmpathNotExistError, "Le chemin de votre ffmpeg n'existe pas." },
            { KeyWords.FfmpegErrorMessage, "Le chemin de votre ffmpeg est invalide : la résolution de votre vidéo pourrait être inférieure." },
            { KeyWords.FfmpathCleared, "Chemin ffmpeg effacé." },
            { KeyWords.SuccessfullySavedData, $"{text} enregistré avec succès." },
            { KeyWords.SuccessfullyDeletedData, $"{text} supprimé avec succès." },
            { KeyWords.SuccessfullyDownloaded, $"{text} téléchargé avec succès." },
            { KeyWords.SuccessfullyUpdatedData, $"{text} mis à jour avec succès."},
            { KeyWords.SuccessfullySavedAllVideos, $"Toutes les vidéos ont été enregistrées avec succès" },
            { KeyWords.OperationCanceled, "Opération annulée" },
            { KeyWords.ErrorWhileSaving, "Une erreur s'est produite lors de l'enregistrement." },
            { KeyWords.ErrorWhileUpdating, "Une erreur s'est produite lors de la mise à jour." },
            { KeyWords.ErrorWhileLoadingPlaylist, "Une erreur s'est produite lors du chargement de votre playlist." },
            { KeyWords.ErrorWhileLoadingData, "Une erreur s'est produite lors du chargement de vos données." },
            { KeyWords.NoLongerExistsDelete, $"{text} n'existe plus. Il a été supprimé." },
            { KeyWords.PleaseEnterAPlaylistUrl, "Veuillez entrer l'URL de la playlist" },
            { KeyWords.ChannelVideoCount, $"Affiche uniquement {text} vidéos." },
            { KeyWords.NoVideoErrorMessage, "Erreur : Vous n'avez aucune vidéo disponible." },
            { KeyWords.DownloadingErrorMessage, $"Il y a eu un problème lors du téléchargement de vos vidéos." },
            { KeyWords.CurrentlyDownloading, $"Téléchargement en cours : {text}" },
            { KeyWords.DownloadPath, "Chemin de téléchargement :" },
            { KeyWords.VideoFormat, "Format vidéo :" },
            { KeyWords.Resolution, "Résolution :" },
            { KeyWords.Language, "Langue :" },
            { KeyWords.FfmpegHelperText, "Entrez le chemin d'accès de votre fichier ffmpeg .exe" },
            { KeyWords.Ffmpeg, "Chemin d'accès du fichier Ffmpeg" },
            { KeyWords.FfmpegSettings, "Paramètres Ffmpeg" },
            { KeyWords.English, "Anglais" },
            { KeyWords.French, "Français" },
            { KeyWords.Indonesian, "Indonésien" },
        };

        return frenchDictionary;
    }
}

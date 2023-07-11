using Microsoft.Extensions.Caching.Memory;

namespace VidFetchLibrary.Language;
public class IndonesianDictionary : IIndonesianDictionary
{
    private const string CacheName = nameof(IndonesianDictionary);
    private const int CacheTime = 5;
    private readonly IMemoryCache _cache;

    public IndonesianDictionary(IMemoryCache cache)
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

    public static Dictionary<KeyWords, string> CreateDictionary(string text)
    {
        var indonesianDictionary = new Dictionary<KeyWords, string>
        {
            { KeyWords.PasteALink, "Tempel Tautan" },
            { KeyWords.SavedMedias, "Media Tersimpan" },
            { KeyWords.SearchMedias , "Cari Media" },
            { KeyWords.Settings, "Pengaturan" },
            { KeyWords.Uploaded, "Diunggah" },
            { KeyWords.Delete , "Hapus" },
            { KeyWords.Save, "Simpan" },
            { KeyWords.Videos, "Video" },
            { KeyWords.Channels, "Saluran" },
            { KeyWords.Playlists, "Daftar Putar" },
            { KeyWords.EnterUrlTextVideo, "Masukkan URL video" },
            { KeyWords.EnterUrlTextChannel, "Masukkan URL saluran" },
            { KeyWords.EnterUrlTextPlaylist, "Masukkan URL daftar putar" },
            { KeyWords.SearchVideo, "Cari video" },
            { KeyWords.SearchChannel, "Cari saluran" },
            { KeyWords.SearchPlaylist, "Cari daftar putar" },
            { KeyWords.SearchVideoPlural, $"Cari {text} video" },
            { KeyWords.SearchChannelPlural, $"Cari {text} saluran" },
            { KeyWords.SearchPlaylistPlural, $"Cari {text} daftar putar" },
            { KeyWords.PlaylistVideos, "Video Daftar Putar" },
            { KeyWords.PlaylistWarning, "Peringatan: Hanya menampilkan 200 video untuk performa." },
            { KeyWords.LoadMore, "Muat Lebih Banyak" },
            { KeyWords.Search, "Cari" },
            { KeyWords.AppSettings, "Pengaturan Aplikasi" },
            { KeyWords.DarkMode, "Mode Gelap" },
            { KeyWords.DownloadSubtitles, "Unduh Subtitle yang disertakan dengan video" },
            { KeyWords.AutoSaveVideo, "Simpan Video Secara Otomatis" },
            { KeyWords.CreateSubdirectory, "Buat Subdirektori untuk playlist" },
            { KeyWords.RemoveVideoDownload, "Hapus Video Setelah Diunduh" },
            { KeyWords.SelectedPath, "Pilih jalur unduhan yang diinginkan." },
            { KeyWords.SelectedFormat, "Pilih format yang diinginkan." },
            { KeyWords.SelectedResolution, "Pilih resolusi yang diinginkan." },
            { KeyWords.SelectedLanguage, "Pilih bahasa yang Anda inginkan." },
            { KeyWords.SearchLabelText, $"Cari {text} Anda" },
            { KeyWords.SearchHelperText, $"Masukkan URL atau judul {text}" },
            { KeyWords.CancelSearch, "Batalkan Pencarian" },
            { KeyWords.OpenFolderLocation, "Buka Lokasi Folder" },
            { KeyWords.Clear, "Bersihkan" },
            { KeyWords.Video, "Video" },
            { KeyWords.Channel, "Saluran" },
            { KeyWords.Playlist, "Playlist" },
            { KeyWords.Download, "Unduh" },
            { KeyWords.Cancel, "Batal" },
            { KeyWords.UpdateData, "Perbarui Data" },
            { KeyWords.CancelUpdateData, "Batalkan Perbarui Data" },
            { KeyWords.DeleteAllWarningTitle, "Hapus Semua" },
            { KeyWords.DeleteAllWarningText, $"Menghapus semua {text} Anda tidak dapat dibatalkan!" },
            { KeyWords.Confirm, "Konfirmasi" },
            { KeyWords.UrlPlaylistTitle, "Sepertinya URL Anda adalah playlist." },
            { KeyWords.UrlPlaylistText, "Apakah Anda ingin mengunduh video dari URL ini?" },
            { KeyWords.Remove, "Hapus" },
            { KeyWords.OpenDetails, "Buka Rincian" },
            { KeyWords.OpenUrl, "Buka URL" },
            { KeyWords.Watch, "Tonton" },
            { KeyWords.SuccessfullySettings, "Pengaturan berhasil disimpan." },
            { KeyWords.FailedSettings, "Gagal menyimpan pengaturan." },
            { KeyWords.FfmpathNotExistError, "Path ffmpeg Anda tidak ada." },
            { KeyWords.FfmpegErrorMessage, "Path ffmpeg Anda tidak valid: Resolusi video Anda mungkin lebih rendah." },
            { KeyWords.FfmpathCleared, "Membersihkan path ffmpeg Anda." },
            { KeyWords.SuccessfullySavedData, $"{text} berhasil disimpan." },
            { KeyWords.SuccessfullyDeletedData, $"{text} berhasil dihapus." },
            { KeyWords.SuccessfullyDownloaded, $"{text} berhasil diunduh." },
            { KeyWords.SuccessfullyUpdatedData, $"{text} berhasil diperbarui."},
            { KeyWords.SuccessfullySavedAllVideos, $"Berhasil menyimpan semua video" },
            { KeyWords.OperationCanceled, "Operasi dibatalkan" },
            { KeyWords.ErrorWhileSaving, "Terjadi kesalahan saat menyimpan." },
            { KeyWords.ErrorWhileUpdating, "Terjadi kesalahan saat memperbarui." },
            { KeyWords.ErrorWhileLoadingPlaylist, "Terjadi kesalahan saat memuat playlist Anda." },
            { KeyWords.ErrorWhileLoadingData, "Terjadi kesalahan saat memuat data Anda." },
            { KeyWords.NoLongerExistsDelete, $"{text} tidak ada lagi. Itu telah dihapus." },
            { KeyWords.PleaseEnterAPlaylistUrl, "Harap masukkan URL playlist" },
            { KeyWords.ChannelVideoCount, $"Hanya menampilkan {text} video." },
            { KeyWords.NoVideoErrorMessage, "Error: Anda tidak memiliki video yang tersedia." },
            { KeyWords.DownloadingErrorMessage, $"Terjadi masalah saat mengunduh video Anda." },
            { KeyWords.CurrentlyDownloading, $"Sedang mengunduh: {text}" },
            { KeyWords.DownloadPath, "Path Unduhan:" },
            { KeyWords.VideoFormat, "Format Video:" },
            { KeyWords.Resolution, "Resolusi:" },
            { KeyWords.Language, "Bahasa:" },
            { KeyWords.FfmpegHelperText, "Masukkan jalur file ffmpeg Anda ke .exe" },
            { KeyWords.Ffmpeg, "Jalur File Ffmpeg" },
            { KeyWords.FfmpegSettings, "Pengaturan Ffmpeg" },
            { KeyWords.SearchTakeLongerWarning, "Pencarian mungkin memakan waktu lebih lama tergantung pada jumlahnya." },
            { KeyWords.Amount, "Jumlah" },
            { KeyWords.English, "Inggris" },
            { KeyWords.French, "Prancis" },
            { KeyWords.Indonesian, "Indonesia" },
        };

        return indonesianDictionary;
    }
}

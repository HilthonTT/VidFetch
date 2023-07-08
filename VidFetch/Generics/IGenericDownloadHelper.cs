namespace VidFetch.Generics;

public interface IGenericDownloadHelper<TData> where TData : class
{
    Task DownloadAllVideosAsync(List<TData> datas, Progress<double> progress, CancellationToken token, Action<TData> RemoveData = null);
}
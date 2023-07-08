namespace VidFetch.Helpers;

public interface IDataHelper<TData> where TData : class
{
    Task DeleteAllAsync(List<TData> datas, Action<TData> RemoveData);
    Task DeleteDataAsync(TData data);
    Task DownloadAllVideosAsync(List<TData> datas, Progress<double> progress, CancellationToken token, Action<TData> RemoveData = default);
    Task<List<TData>> LoadDataAsync(string url, int loadedItems);
    Task SaveAllAsync(List<TData> datas, CancellationToken token);
    Task UpdateAllAsync(List<TData> datas, Action<TData> RemoveData, CancellationToken token);
}
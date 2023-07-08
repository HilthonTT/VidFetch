namespace VidFetch.Generics;

public interface IGenericGeneralHelper<TData> where TData : class
{
    void ClearDatas();
    Task DeleteAllAsync(List<TData> datas, Action<TData> removeData);
    Task DeleteAsync(TData data);
    Task DownloadAllAsync(List<TData> datas, Progress<double> progress, CancellationToken token, Action<TData> removeData = default);
    string GetName();
    Task<List<TData>> LoadDataAsync(string url, int loadedItems);
    void RemoveData(TData data);
    Task SaveDataAsync(List<TData> datas, CancellationToken token);
    Task UpdateAllDataAsync(List<TData> datas, CancellationToken token, Action<TData> removeData = null);
    Task UpdateDataAsync(TData data, Action<TData> removeData, CancellationToken token);
}
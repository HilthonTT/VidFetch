namespace VidFetch.Generics;

public interface IGenericUpdateHelper<TData> where TData : class
{
    Task UpdateAllAsync(List<TData> datas, CancellationToken token, Action<TData> RemoveData);
    Task UpdateDataAsync(TData data, Action<TData> RemoveData, CancellationToken token);
}
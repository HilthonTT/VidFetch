namespace VidFetch.Generics;

public interface IGenericDeleteHelper<TData> where TData : class
{
    Task DeleteAllAsync(List<TData> datas, Action<TData> RemoveData);
    Task DeleteDataAsync(TData data);
    string GetName();
}
namespace VidFetch.Generics;

public interface IGenericSaveHelper<TData> where TData : class
{
    string GetName();
    Task SaveAllAsync(List<TData> datas, CancellationToken token);
}
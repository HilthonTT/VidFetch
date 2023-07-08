namespace VidFetch.Generics;

public interface IGenericLoadHelper<TData> where TData : class
{
    Task<List<TData>> LoadDataAsync(string url, int loadedItems);
}
using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public class AuthorData : IAuthorData
{
    private const string DbName = "Author.db3";
    private const string CacheName = "AuthorData";
    private readonly IMemoryCache _cache;
    private SQLiteAsyncConnection _db;

    public AuthorData(IMemoryCache cache)
    {
        _cache = cache;
        SetUpDb();
    }

    private void SetUpDb()
    {
        if (_db is null)
        {
            string dbPath = Path
                .Combine(Environment
                .GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), DbName);

            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<AuthorModel>();
        }
    }

    public async Task<List<AuthorModel>> GetAuthorsAsync()
    {
        var output = _cache.Get<List<AuthorModel>>(CacheName);
        if (output is null)
        {
            output = await _db.Table<AuthorModel>().ToListAsync();
            _cache.Set(CacheName, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<int> AddAuthorAsync(AuthorModel author)
    {
        RemoveCache();
        return await _db.InsertAsync(author);
    }

    public async Task<int> UpdateAuthorAsync(AuthorModel author)
    {
        RemoveCache();
        return await _db.UpdateAsync(author);
    }

    public async Task<int> DeleteAuthorAsync(AuthorModel author)
    {
        RemoveCache();
        return await _db.DeleteAsync(author);
    }

    private void RemoveCache()
    {
        _cache.Remove(CacheName);
    }
}

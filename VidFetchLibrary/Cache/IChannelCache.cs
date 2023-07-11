using VidFetchLibrary.Models;

namespace VidFetchLibrary.Cache;
public interface IChannelCache
{
    Task<ChannelModel> GetChannelAsync(string url, CancellationToken token = default);
    Task<List<ChannelModel>> GetChannelBySearchAsync(string searchInput, int count, CancellationToken token = default);
}
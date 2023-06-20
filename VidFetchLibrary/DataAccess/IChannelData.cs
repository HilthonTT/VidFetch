using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public interface IChannelData
{
    Task<bool> ChannelExistsAsync(string url, string channelId);
    Task<int> DeleteChannelAsync(ChannelModel channel);
    Task<List<ChannelModel>> GetAllChannelsAsync();
    Task<ChannelModel> GetChannelAsync(string url, string channelId);
    Task<int> SetChannelAsync(string url, string channelId);
}
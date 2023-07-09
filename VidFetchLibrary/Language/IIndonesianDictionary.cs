namespace VidFetchLibrary.Language;

public interface IIndonesianDictionary
{
    Dictionary<KeyWords, string> GetDictionary(string text);
}
namespace VidFetchLibrary.Language;

public interface IEnglishDictionary
{
    Dictionary<KeyWords, string> GetDictionary(string text);
}
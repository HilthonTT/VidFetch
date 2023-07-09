namespace VidFetchLibrary.Language;

public interface IFrenchDictionary
{
    Dictionary<KeyWords, string> GetDictionary(string text);
}
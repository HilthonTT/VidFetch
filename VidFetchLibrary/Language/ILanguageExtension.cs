namespace VidFetchLibrary.Language;

public interface ILanguageExtension
{
    Dictionary<KeyWords, string> GetDictionary(string text = "");
}
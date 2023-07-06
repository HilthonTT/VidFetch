using VidFetchLibrary.Language;

namespace VidFetch.Pages;

public partial class Search
{
    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = languageExtension.GetDictionary();
        return dictionary;
    }
}
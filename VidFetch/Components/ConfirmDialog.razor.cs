using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Language;

namespace VidFetch.Components;

public partial class ConfirmDialog
{
    [Parameter]
    [EditorRequired]
    public bool IsVisible { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment TitleContent { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment Content { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback Cancel { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback Confirm { get; set; }

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = languageExtension.GetDictionary();
        return dictionary;
    }
}
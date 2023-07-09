namespace VidFetch.Services;
public class ClipboardWrapper : IClipboard
{
    public bool HasText => throw new NotImplementedException();

    public event EventHandler<EventArgs> ClipboardContentChanged;

    public async Task<string> GetTextAsync()
    {
        return await Clipboard.GetTextAsync();
    }

    public async Task SetTextAsync(string text)
    {
        await Clipboard.SetTextAsync(text);
    }
}

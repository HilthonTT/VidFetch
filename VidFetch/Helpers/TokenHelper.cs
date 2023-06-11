namespace VidFetch.Helpers;
public class TokenHelper : ITokenHelper
{
    public CancellationToken InitializeToken(ref CancellationTokenSource tokenSource)
    {
        tokenSource = new();
        return tokenSource.Token;
    }

    public void CancelRequest(ref CancellationTokenSource tokenSource)
    {
        tokenSource?.Cancel();
        tokenSource = null;
    }
}

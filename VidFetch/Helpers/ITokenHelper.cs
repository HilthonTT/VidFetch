namespace VidFetch.Helpers;

public interface ITokenHelper
{
    void CancelRequest(ref CancellationTokenSource tokenSource);
    CancellationToken InitializeToken(ref CancellationTokenSource tokenSource);
}
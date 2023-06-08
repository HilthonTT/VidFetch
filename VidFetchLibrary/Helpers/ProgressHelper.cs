namespace VidFetchLibrary.Helpers;
public class ProgressHelper : IProgress<double>
{
    public double CurrentProgress { get; set; }

    public void Report(double value)
    {
        CurrentProgress = value;
    }

    public double GetCurrentProgress()
    {
        return CurrentProgress;
    }
}

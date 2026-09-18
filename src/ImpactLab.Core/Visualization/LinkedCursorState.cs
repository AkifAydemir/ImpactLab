namespace ImpactLab.Core.Visualization;

public sealed class LinkedCursorState
{
    public double TimeSeconds { get; private set; }
    public event EventHandler<double>? Changed;

    public void Set(double timeSeconds)
    {
        if (Math.Abs(TimeSeconds - timeSeconds) <= 1e-12)
            return;
        TimeSeconds = Math.Max(0, timeSeconds);
        Changed?.Invoke(this, TimeSeconds);
    }
}

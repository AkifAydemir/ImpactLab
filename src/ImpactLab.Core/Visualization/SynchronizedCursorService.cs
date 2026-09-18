namespace ImpactLab.Core.Visualization;

public sealed class SynchronizedCursorService
{
    private double? _time;
    public double? TimeSeconds => _time;
    public event EventHandler<double?>? Changed;

    public void Set(double? time)
    {
        if (_time == time)
            return;
        _time = time;
        Changed?.Invoke(this, time);
    }

    public void Clear() => Set(null);
}

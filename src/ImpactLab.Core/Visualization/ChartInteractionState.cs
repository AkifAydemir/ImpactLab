namespace ImpactLab.Core.Visualization;

public sealed class ChartInteractionState
{
    public ChartViewport? Viewport { get; private set; }
    public double? CursorX { get; private set; }
    public event EventHandler? Changed;

    public void Fit(ChartDocument document)
    {
        Viewport = ChartViewport.Auto(document);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetViewport(ChartViewport v)
    {
        Viewport = v;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetCursor(double? x)
    {
        CursorX = x;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

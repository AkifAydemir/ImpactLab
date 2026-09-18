namespace ImpactLab.Core.Visualization;

public sealed record ChartViewport(double XMin, double XMax, double YMin, double YMax)
{
    public double XSpan => Math.Max(XMax - XMin, 1e-15);
    public double YSpan => Math.Max(YMax - YMin, 1e-15);

    public ChartViewport Pan(double dx, double dy) =>
        new(XMin + dx, XMax + dx, YMin + dy, YMax + dy);

    public ChartViewport Zoom(double centerX, double centerY, double factor)
    {
        factor = Math.Clamp(factor, 0.01, 100);
        var hx = XSpan * factor * 0.5;
        var hy = YSpan * factor * 0.5;
        return new(centerX - hx, centerX + hx, centerY - hy, centerY + hy);
    }

    public static ChartViewport Auto(ChartDocument d)
    {
        var p = d.Series.SelectMany(x => x.Points).ToArray();
        if (p.Length == 0)
            return new(0, 1, 0, 1);
        var xmin = p.Min(x => x.X);
        var xmax = p.Max(x => x.X);
        var ymin = p.Min(x => x.Y);
        var ymax = p.Max(x => x.Y);
        if (Math.Abs(xmax - xmin) < 1e-15)
            xmax = xmin + 1;
        if (Math.Abs(ymax - ymin) < 1e-15)
            ymax = ymin + 1;
        return new(xmin, xmax, ymin, ymax);
    }
}

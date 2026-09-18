namespace ImpactLab.App.Authoring.Gizmo;

public static class GizmoSnapService
{
    public static double Snap(double value, double step) =>
        step <= 0 ? value : Math.Round(value / step) * step;
}

using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Authoring;

public sealed class GizmoDragSession
{
    public GizmoDragSession(
        GizmoMode mode,
        GizmoAxis axis,
        GeometryTransform3 initial,
        Vec3 pivot,
        Ray3 startRay,
        SnapSettings snap
    )
    {
        Mode = mode;
        Axis = axis;
        Initial = initial;
        Pivot = pivot;
        StartRay = startRay;
        Snap = snap;
    }

    public GizmoMode Mode { get; }
    public GizmoAxis Axis { get; }
    public GeometryTransform3 Initial { get; }
    public Vec3 Pivot { get; }
    public Ray3 StartRay { get; }
    public SnapSettings Snap { get; }

    public GizmoDragResult Update(Ray3 ray)
    {
        var axis = AxisVector(Axis);
        var delta =
            ClosestAxisParameter(ray, Pivot, axis) - ClosestAxisParameter(StartRay, Pivot, axis);
        delta = TransformSnapper.Snap(delta, Snap.TranslationStepMeters, Snap.TranslationEnabled);
        if (Mode == GizmoMode.Translate)
        {
            var d = axis * delta;
            return new(Initial with { Translation = Initial.Translation + d }, d, delta);
        }
        if (Mode == GizmoMode.Scale)
        {
            var factor = Math.Max(0.01, 1 + delta);
            factor = TransformSnapper.Snap(factor, Snap.ScaleStep, Snap.ScaleEnabled);
            var s = Axis switch
            {
                GizmoAxis.X => new Vec3(factor, 1, 1),
                GizmoAxis.Y => new Vec3(1, factor, 1),
                GizmoAxis.Z => new Vec3(1, 1, factor),
                _ => new Vec3(factor, factor, factor),
            };
            return new(
                Initial with
                {
                    Scale = new Vec3(
                        Initial.Scale.X * s.X,
                        Initial.Scale.Y * s.Y,
                        Initial.Scale.Z * s.Z
                    ),
                },
                Vec3.Zero,
                factor
            );
        }
        var angle = TransformSnapper.Snap(delta, Snap.RotationStepRadians, Snap.RotationEnabled);
        var q = QuaternionD.FromAxisAngle(axis, angle) * Initial.Rotation;
        return new(Initial with { Rotation = q.Normalized() }, Vec3.Zero, angle);
    }

    private static Vec3 AxisVector(GizmoAxis axis) =>
        axis switch
        {
            GizmoAxis.X => Vec3.UnitX,
            GizmoAxis.Y => Vec3.UnitY,
            GizmoAxis.Z => Vec3.UnitZ,
            GizmoAxis.XY => new Vec3(1, 1, 0).Normalized(),
            GizmoAxis.YZ => new Vec3(0, 1, 1).Normalized(),
            GizmoAxis.ZX => new Vec3(1, 0, 1).Normalized(),
            _ => new Vec3(1, 1, 1).Normalized(),
        };

    private static double ClosestAxisParameter(Ray3 ray, Vec3 origin, Vec3 axis)
    {
        var d = ray.Direction.Normalized();
        var w0 = ray.Origin - origin;
        var a = Vec3.Dot(d, d);
        var b = Vec3.Dot(d, axis);
        var c = Vec3.Dot(axis, axis);
        var d0 = Vec3.Dot(d, w0);
        var e = Vec3.Dot(axis, w0);
        var den = a * c - b * b;
        return Math.Abs(den) < 1e-12 ? -e / c : (a * e - b * d0) / den;
    }
}

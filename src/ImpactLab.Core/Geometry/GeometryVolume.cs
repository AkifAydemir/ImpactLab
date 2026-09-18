namespace ImpactLab.Core.Geometry;

public static class GeometryVolume
{
    public static double Calculate(GeometrySpec geometry) =>
        geometry switch
        {
            RectangularPlateSpec plate => plate.Width * plate.Height * plate.Thickness,
            BoxSpec box => box.Size.X * box.Size.Y * box.Size.Z,
            CylinderSpec cylinder => Math.PI * cylinder.Radius * cylinder.Radius * cylinder.Length,
            TubeSpec tube => Math.PI
                * (tube.OuterRadius * tube.OuterRadius - tube.InnerRadius * tube.InnerRadius)
                * tube.Length,
            AngleProfileSpec angle => (
                angle.LegX * angle.Thickness
                + angle.LegY * angle.Thickness
                - angle.Thickness * angle.Thickness
            ) * angle.Length,
            SphereSpec sphere => 4.0
                / 3.0
                * Math.PI
                * sphere.Radius
                * sphere.Radius
                * sphere.Radius,
            _ => throw new NotSupportedException(
                $"Volume calculation is not implemented for {geometry.GetType().Name}."
            ),
        };
}

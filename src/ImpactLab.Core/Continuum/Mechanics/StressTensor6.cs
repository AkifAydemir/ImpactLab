namespace ImpactLab.Core.Continuum.Mechanics;

public readonly record struct StressTensor6(
    double XX,
    double YY,
    double ZZ,
    double XY,
    double YZ,
    double ZX
)
{
    public double VonMises =>
        Math.Sqrt(
            0.5
                * (
                    (XX - YY) * (XX - YY)
                    + (YY - ZZ) * (YY - ZZ)
                    + (ZZ - XX) * (ZZ - XX)
                    + 6 * (XY * XY + YZ * YZ + ZX * ZX)
                )
        );

    public double[] ToArray() => [XX, YY, ZZ, XY, YZ, ZX];
}

namespace ImpactLab.Core.Constraints;

[Flags]
public enum AxisLockMask
{
    None = 0,
    X = 1,
    Y = 2,
    Z = 4,
    All = X | Y | Z,
}

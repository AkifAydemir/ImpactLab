namespace ImpactLab.Core.Continuum.Dynamics;

public static class DynamicEnergyCalculator
{
    public static double Kinetic(ContinuumDynamicState s, double[] mass)
    {
        double e = 0;
        for (var i = 0; i < s.Velocity.Length; i++)
        {
            var j = i * 3;
            var v = s.Velocity[i];
            e += 0.5 * (mass[j] * v.X * v.X + mass[j + 1] * v.Y * v.Y + mass[j + 2] * v.Z * v.Z);
        }
        return e;
    }

    public static double Internal(ContinuumDynamicState s) =>
        s.ElementStates.Sum(x => x.InternalEnergyJ);

    public static double Plastic(ContinuumDynamicState s) =>
        s.ElementStates.Sum(x => x.DissipatedEnergyJ);
}

namespace ImpactLab.Core.Experiments.Uncertainty;

public static class DistributionSampler
{
    public static double Sample(ParameterDistribution d, double u, double z = 0)
    {
        return d.Kind switch
        {
            DistributionKind.Uniform => d.A + (d.B - d.A) * u,
            DistributionKind.Normal => d.A + d.B * z,
            DistributionKind.LogNormal => Math.Exp(d.A + d.B * z),
            DistributionKind.Triangular => u < (d.C - d.A) / (d.B - d.A)
                ? d.A + Math.Sqrt(u * (d.B - d.A) * (d.C - d.A))
                : d.B - Math.Sqrt((1 - u) * (d.B - d.A) * (d.B - d.C)),
            DistributionKind.Discrete => d.DiscreteValues![
                Math.Min(d.DiscreteValues.Count - 1, (int)(u * d.DiscreteValues.Count))
            ],
            _ => d.A,
        };
    }
}

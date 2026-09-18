namespace ImpactLab.Core.Experiments.Sensitivity;

public sealed class SaltelliSampler
{
    public double[][] Generate(SaltelliSamplingPlan p)
    {
        var rng = new Random(p.Seed);
        var rows = p.EvaluationCount;
        var x = new double[rows][];
        for (var i = 0; i < rows; i++)
        {
            x[i] = new double[p.ParameterCount];
            for (var j = 0; j < p.ParameterCount; j++)
                x[i][j] = rng.NextDouble();
        }
        return x;
    }
}

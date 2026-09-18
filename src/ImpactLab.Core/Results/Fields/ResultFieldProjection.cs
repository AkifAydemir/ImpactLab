namespace ImpactLab.Core.Results.Fields;

public static class ResultFieldProjection
{
    public static double[] Magnitude(ReadOnlySpan<double> values, int components)
    {
        if (components < 1 || values.Length % components != 0)
            throw new ArgumentException(
                "Component count must be positive and divide the input length.",
                nameof(components)
            );

        var result = new double[values.Length / components];
        for (var index = 0; index < result.Length; index++)
        {
            double sumOfSquares = 0;
            for (var component = 0; component < components; component++)
            {
                var value = values[index * components + component];
                sumOfSquares += value * value;
            }
            result[index] = Math.Sqrt(sumOfSquares);
        }
        return result;
    }
}

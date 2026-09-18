namespace ImpactLab.Core.Experiments.Optimization;

public sealed class BayesianOptimizer
{
    private readonly Random _random;

    public BayesianOptimizer(int seed = 12345) => _random = new(seed);

    public OptimizationTrace Run(
        IReadOnlyList<(string Name, double Min, double Max)> bounds,
        int iterations,
        Func<IReadOnlyDictionary<string, double>, double> objective
    )
    {
        var points = new List<double[]>();
        var values = new List<double>();
        var evaluations = new List<OptimizationEvaluation>();

        for (var iteration = 0; iteration < iterations; iteration++)
        {
            double[] point;
            if (points.Count < Math.Max(4, bounds.Count * 2))
            {
                point = bounds
                    .Select(bound => bound.Min + _random.NextDouble() * (bound.Max - bound.Min))
                    .ToArray();
            }
            else
            {
                point = Suggest(bounds, points, values);
            }

            var parameters = bounds
                .Select((bound, index) => (bound.Name, point[index]))
                .ToDictionary(pair => pair.Name, pair => pair.Item2);
            var value = objective(parameters);
            points.Add(point);
            values.Add(value);
            evaluations.Add(new(iteration, parameters, value, true));
        }

        return new(evaluations.OrderBy(evaluation => evaluation.Objective).First(), evaluations);
    }

    private double[] Suggest(
        IReadOnlyList<(string Name, double Min, double Max)> bounds,
        List<double[]> points,
        List<double> values
    )
    {
        var surrogate = new GaussianProcessSurrogate(new());
        surrogate.Fit(points, values);
        var bestValue = values.Min();
        double[]? bestPoint = null;
        var bestImprovement = double.NegativeInfinity;

        for (var candidate = 0; candidate < 256; candidate++)
        {
            var point = bounds
                .Select(bound => bound.Min + _random.NextDouble() * (bound.Max - bound.Min))
                .ToArray();
            var prediction = surrogate.Predict(point);
            var improvement = ExpectedImprovement.Evaluate(
                prediction.Mean,
                prediction.Variance,
                bestValue
            );
            if (improvement > bestImprovement)
            {
                bestImprovement = improvement;
                bestPoint = point;
            }
        }

        return bestPoint!;
    }
}

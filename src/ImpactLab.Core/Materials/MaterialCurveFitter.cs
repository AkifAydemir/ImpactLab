namespace ImpactLab.Core.Materials;

public static class MaterialCurveFitter
{
    public static MaterialProfile FitBilinear(
        MaterialDefinition seed,
        MaterialCalibrationDataset dataset,
        string derivedId
    )
    {
        dataset.Validate();
        var samples = dataset.Samples.OrderBy(x => x.Strain).ToArray();
        var elastic = samples
            .Where(x => x.Strain > 0.0)
            .Take(Math.Max(2, samples.Length / 4))
            .ToArray();
        var e = WeightedSlope(elastic);
        var maxStress = samples.Max(x => x.StressPa);
        var yieldSample = samples.FirstOrDefault(x =>
            x.Strain > 0.0 && Math.Abs(x.StressPa - e * x.Strain) > Math.Max(maxStress * 0.02, 1.0)
        );
        var yieldStrain =
            yieldSample.Strain > 0.0
                ? yieldSample.Strain
                : Math.Max(seed.YieldStrain, samples[samples.Length / 3].Strain);
        var failureStrain = Math.Max(yieldStrain * 1.05, samples.Max(x => x.Strain));
        var post = samples.Where(x => x.Strain >= yieldStrain).ToArray();
        var tangent = post.Length >= 2 ? Math.Max(0.0, WeightedSlope(post)) : seed.TangentModulusPa;
        var fitted = seed with
        {
            Id = derivedId,
            DisplayName = $"{seed.DisplayName} (calibrated)",
            YoungModulusPa = Math.Max(e, 1.0),
            YieldStrain = yieldStrain,
            FailureStrain = failureStrain,
            TangentModulusPa = tangent,
        };
        var curve = new ConstitutiveCurve(
            samples.Select(x => new ConstitutiveCurvePoint(x.Strain, x.StressPa))
        );
        var errors = samples
            .Select(x =>
                (Actual: x.StressPa, Predicted: Predict(fitted, x.Strain), Weight: x.Weight)
            )
            .ToArray();
        var rmse = Math.Sqrt(
            errors.Sum(x => x.Weight * Math.Pow(x.Predicted - x.Actual, 2))
                / errors.Sum(x => x.Weight)
        );
        var mae =
            errors.Sum(x => x.Weight * Math.Abs(x.Predicted - x.Actual))
            / errors.Sum(x => x.Weight);
        var mean = errors.Average(x => x.Actual);
        var ssTot = errors.Sum(x => Math.Pow(x.Actual - mean, 2));
        var ssRes = errors.Sum(x => Math.Pow(x.Predicted - x.Actual, 2));
        var r2 = ssTot <= 1e-18 ? 1.0 : 1.0 - ssRes / ssTot;
        return new MaterialProfile(fitted)
        {
            TensileCurve = curve,
            Provenance = dataset.Provenance,
            Calibration = new MaterialCalibrationMetrics(
                rmse,
                mae,
                r2,
                samples.Length,
                "Bilinear elastic-plastic fit"
            ),
        };
    }

    private static double WeightedSlope(IReadOnlyList<MaterialTestSample> samples)
    {
        var numerator = samples.Sum(x => x.Weight * x.Strain * x.StressPa);
        var denominator = samples.Sum(x => x.Weight * x.Strain * x.Strain);
        return denominator <= 1e-24 ? 0.0 : numerator / denominator;
    }

    private static double Predict(MaterialDefinition m, double strain)
    {
        var sign = Math.Sign(strain);
        var a = Math.Abs(strain);
        if (a <= m.YieldStrain)
            return m.YoungModulusPa * strain;
        var yieldStress = m.YoungModulusPa * m.YieldStrain;
        return sign * (yieldStress + m.TangentModulusPa * (a - m.YieldStrain));
    }
}

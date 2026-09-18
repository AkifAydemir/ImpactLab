namespace ImpactLab.Core.Parameters;

public sealed record ParameterDefinition(
    string Key,
    string DisplayName,
    string Unit,
    double DefaultValue,
    double Minimum,
    double Maximum,
    double Step,
    ParameterValueKind ValueKind = ParameterValueKind.Scalar
)
{
    public void ValidateDefinition()
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new InvalidOperationException("Parameter key cannot be empty.");
        if (Maximum < Minimum)
            throw new InvalidOperationException($"Invalid range for {Key}.");
        if (DefaultValue < Minimum || DefaultValue > Maximum)
            throw new InvalidOperationException($"Default value for {Key} is outside its range.");
        if (Step <= 0.0)
            throw new InvalidOperationException($"Step for {Key} must be positive.");
    }

    public double Normalize(double value)
    {
        var clamped = Math.Clamp(value, Minimum, Maximum);
        return ValueKind switch
        {
            ParameterValueKind.Integer => Math.Round(clamped),
            ParameterValueKind.Boolean => clamped >= 0.5 ? 1.0 : 0.0,
            _ => clamped,
        };
    }
}

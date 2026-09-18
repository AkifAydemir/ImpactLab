namespace ImpactLab.Core.Continuum.Dynamics;

public sealed record ContinuumDampingSettings(
    double MassCoefficient = 0.0,
    double StiffnessCoefficient = 0.0,
    double NumericalDamping = 0.0
)
{
    public void Validate()
    {
        if (
            MassCoefficient < 0
            || StiffnessCoefficient < 0
            || NumericalDamping < 0
            || NumericalDamping >= 1
        )
            throw new InvalidOperationException("Invalid continuum damping settings.");
    }
}

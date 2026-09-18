namespace ImpactLab.Core.Contact;

public sealed record ContactPairRule(
    string RigidBodyId,
    string TargetPartId,
    bool Enabled = true,
    double NormalStiffnessScale = 1.0,
    double NormalDampingScale = 1.0,
    double FrictionScale = 1.0
)
{
    public void Validate()
    {
        if (NormalStiffnessScale <= 0.0)
            throw new InvalidOperationException("Contact stiffness scale must be positive.");
        if (NormalDampingScale < 0.0)
            throw new InvalidOperationException("Contact damping scale cannot be negative.");
        if (FrictionScale < 0.0)
            throw new InvalidOperationException("Friction scale cannot be negative.");
    }
}

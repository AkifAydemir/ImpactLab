namespace ImpactLab.Core.Scenarios;

public sealed record ScenarioContactDefinition(
    string RigidBodyId,
    string TargetPartId,
    bool Enabled = true,
    double NormalStiffnessScale = 1.0,
    double NormalDampingScale = 1.0,
    double FrictionScale = 1.0
);

namespace ImpactLab.Core.Contact;

public sealed record DeformableContactRule(
    string PartA,
    string PartB,
    bool Enabled = true,
    double StiffnessScale = 0.35,
    double DampingScale = 0.5,
    double FrictionScale = 0.75
)
{
    public string Key =>
        string.CompareOrdinal(PartA, PartB) <= 0 ? $"{PartA}|{PartB}" : $"{PartB}|{PartA}";
}

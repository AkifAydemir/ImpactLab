namespace ImpactLab.Core.Verification;

public sealed class VerificationSuite
{
    public string Name { get; set; } = "Verification suite";
    public List<VerificationCase> Cases { get; } = [];
}

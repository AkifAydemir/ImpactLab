namespace ImpactLab.Core.Contact;

public sealed class DeformableContactTable
{
    private readonly Dictionary<string, DeformableContactRule> _rules = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Set(DeformableContactRule rule) => _rules[rule.Key] = rule;

    public DeformableContactRule Resolve(string partA, string partB)
    {
        var probe = new DeformableContactRule(partA, partB);
        return _rules.TryGetValue(probe.Key, out var rule) ? rule : probe;
    }
}

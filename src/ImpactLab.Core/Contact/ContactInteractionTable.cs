namespace ImpactLab.Core.Contact;

public sealed class ContactInteractionTable
{
    private readonly Dictionary<(string BodyId, string PartId), ContactPairRule> _rules = [];

    public void Set(ContactPairRule rule)
    {
        rule.Validate();
        _rules[(rule.RigidBodyId, rule.TargetPartId)] = rule;
    }

    public ContactPairRule Resolve(string rigidBodyId, string partId)
    {
        if (_rules.TryGetValue((rigidBodyId, partId), out var rule))
            return rule;
        return new ContactPairRule(rigidBodyId, partId);
    }
}

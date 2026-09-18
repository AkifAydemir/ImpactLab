namespace ImpactLab.Core.Continuum.Contact;

public sealed class ContactMultiplierStore
{
    private readonly Dictionary<ContactConstraintKey, ContactMultiplierState> _s = [];

    public ContactMultiplierState Get(ContactConstraintKey k) =>
        _s.TryGetValue(k, out var v) ? v : new(0, 0, 0, true, 0, 0);

    public void Set(ContactConstraintKey k, ContactMultiplierState v) => _s[k] = v;

    public void RemoveInactive(IReadOnlySet<ContactConstraintKey> active) =>
        _s.Keys.Where(k => !active.Contains(k)).ToList().ForEach(k => _s.Remove(k));

    public IReadOnlyDictionary<ContactConstraintKey, ContactMultiplierState> Snapshot() =>
        new Dictionary<ContactConstraintKey, ContactMultiplierState>(_s);
}

namespace ImpactLab.Core.Continuum.Contact;

public sealed class ContactHistoryStore
{
    private readonly Dictionary<ContactHistoryKey, ContactHistoryState> _s = [];

    public ContactHistoryState Get(ContactHistoryKey k) =>
        _s.TryGetValue(k, out var v) ? v : new(default, 0, 0, 0);

    public void Set(ContactHistoryKey k, ContactHistoryState v) => _s[k] = v;

    public void RemoveMissing(HashSet<ContactHistoryKey> active)
    {
        foreach (var k in _s.Keys.Where(k => !active.Contains(k)).ToArray())
            _s.Remove(k);
    }

    public IReadOnlyDictionary<ContactHistoryKey, ContactHistoryState> Snapshot() =>
        new Dictionary<ContactHistoryKey, ContactHistoryState>(_s);
}

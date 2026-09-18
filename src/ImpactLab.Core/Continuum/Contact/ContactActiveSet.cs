namespace ImpactLab.Core.Continuum.Contact;

public sealed class ContactActiveSet
{
    private readonly HashSet<ContactConstraintKey> _active = [];
    public IReadOnlySet<ContactConstraintKey> Keys => _active;

    public bool Update(IEnumerable<ContactConstraintKey> keys)
    {
        var n = keys.ToHashSet();
        var changed = !n.SetEquals(_active);
        _active.Clear();
        _active.UnionWith(n);
        return changed;
    }
}

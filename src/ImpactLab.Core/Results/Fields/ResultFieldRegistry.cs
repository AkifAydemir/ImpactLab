namespace ImpactLab.Core.Results.Fields;

public sealed class ResultFieldRegistry
{
    private readonly Dictionary<string, ResultFieldDescriptor> _fields = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register(ResultFieldDescriptor d)
    {
        d.Validate();
        _fields[d.Id] = d;
    }

    public ResultFieldDescriptor Get(string id) =>
        _fields.TryGetValue(id, out var d) ? d : throw new KeyNotFoundException(id);

    public IReadOnlyList<ResultFieldDescriptor> All =>
        _fields.Values.OrderBy(x => x.DisplayName).ToArray();

    public bool Contains(string id) => _fields.ContainsKey(id);

    public static ResultFieldRegistry CreateDefault()
    {
        var r = new ResultFieldRegistry();
        foreach (var d in BuiltInResultFields.All)
            r.Register(d);
        return r;
    }
}

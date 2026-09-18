namespace ImpactLab.Core.Results.Fields;

public sealed record ResultFieldDescriptor(
    string Id,
    string DisplayName,
    string Unit,
    ResultFieldAssociation Association,
    ResultFieldValueType ValueType,
    int Components,
    bool TimeDependent = true,
    string? Description = null
)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id) || Components < 1)
            throw new InvalidOperationException("Invalid result field descriptor.");
    }
}

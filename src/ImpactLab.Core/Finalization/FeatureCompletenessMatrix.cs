namespace ImpactLab.Core.Finalization;

public sealed class FeatureCompletenessMatrix
{
    private readonly List<FeatureReadinessItem> _items = [];

    public IReadOnlyList<FeatureReadinessItem> Items => _items;

    public void Set(FeatureReadinessItem item)
    {
        _items.RemoveAll(x => x.Area == item.Area);
        _items.Add(item);
    }

    public IReadOnlyList<FeatureArea> MissingAreas =>
        Enum.GetValues<FeatureArea>().Where(area => _items.All(x => x.Area != area)).ToArray();

    public bool HasCompleteAreaSet =>
        MissingAreas.Count == 0 && _items.Count == Enum.GetValues<FeatureArea>().Length;

    public bool IsFeatureComplete =>
        HasCompleteAreaSet
        && _items
            .Where(x => x.RequiredForCompletion)
            .All(x => x.Status is FeatureAreaStatus.Ready or FeatureAreaStatus.ExternalDependency);

    public bool IsSourceFeatureComplete =>
        HasCompleteAreaSet
        && _items
            .Where(x => x.RequiredForCompletion)
            .All(x => x.Status is not FeatureAreaStatus.Planned);

    public IReadOnlyList<FeatureReadinessItem> Blocking =>
        _items
            .Where(x =>
                x.RequiredForCompletion
                && x.Status is not (FeatureAreaStatus.Ready or FeatureAreaStatus.ExternalDependency)
            )
            .ToArray();

    public IReadOnlyList<FeatureReadinessItem> SourceBlocking =>
        _items
            .Where(x => x.RequiredForCompletion && x.Status == FeatureAreaStatus.Planned)
            .ToArray();
}

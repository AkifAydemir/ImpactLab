namespace ImpactLab.Core.Results.Storage;

public sealed record ResultPrefetchItem(string FieldId, int FrameIndex, int Priority);

public sealed record ResultPrefetchPlan(IReadOnlyList<ResultPrefetchItem> Items);

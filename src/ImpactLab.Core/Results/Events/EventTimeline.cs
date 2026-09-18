namespace ImpactLab.Core.Results.Events;

public sealed record EventTimeline(IReadOnlyList<ResultEvent> Events)
{
    public IReadOnlyList<ResultEvent> Critical =>
        Events.Where(x => x.Severity == ResultEventSeverity.Critical).ToArray();
}

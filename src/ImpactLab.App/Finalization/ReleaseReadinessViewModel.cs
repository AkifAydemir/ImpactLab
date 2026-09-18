using ImpactLab.Core.Finalization;

namespace ImpactLab.App.Finalization;

public sealed class ReleaseReadinessViewModel
{
    public IReadOnlyList<FeatureReadinessItem> Items { get; private set; } = [];
    public bool SourceFeatureComplete { get; private set; }
    public bool ReleaseFeatureComplete { get; private set; }
    public ReleaseReadinessReport? ReleaseReport { get; private set; }

    public void Refresh(ReleaseReadinessReport? releaseReport = null)
    {
        var matrix = FeatureCompletenessReview.CreateV16SourceBaseline();
        Items = matrix.Items;
        SourceFeatureComplete = matrix.IsSourceFeatureComplete;
        ReleaseReport = releaseReport;
        ReleaseFeatureComplete = releaseReport?.Ready ?? false;
    }
}

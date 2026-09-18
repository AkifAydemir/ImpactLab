using ImpactLab.Core.Finalization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class FeatureCompletenessTests
{
    [Fact]
    public void V12BaselineStillHasVerificationGates()
    {
        var m = FeatureCompletenessReview.CreateV12Baseline();
        Assert.False(m.IsFeatureComplete);
        Assert.NotEmpty(m.Blocking);
    }
}

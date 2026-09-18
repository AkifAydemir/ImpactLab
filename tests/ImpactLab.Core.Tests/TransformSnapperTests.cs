using ImpactLab.Core.Authoring;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class TransformSnapperTests
{
    [Fact]
    public void TranslationSnapsToGrid()
    {
        var r = TransformSnapper.Snap(
            new TransformDelta(new Vec3(0.0014, 0, 0), Vec3.Zero, new Vec3(1, 1, 1)),
            new SnapSettings(true, 0.001, 5, 0.05)
        );
        Assert.Equal(0.001, r.Translation.X, 8);
    }
}

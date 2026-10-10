using ImpactLab.Core.Fracture;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class FragmentLineageTrackerTests
{
    [Fact]
    public void RepeatedSplitsRetainParentAndGenerationHistory()
    {
        var tracker = new FragmentLineageTracker();

        var roots = tracker.Observe([Fragment("part:0", [0, 1, 2, 3])], 0, 0);
        var firstSplit = tracker.Observe(
            [Fragment("part:0", [0, 1]), Fragment("part:2", [2, 3])],
            10,
            0.01
        );
        var secondSplit = tracker.Observe(
            [Fragment("part:0", [0, 1]), Fragment("part:2", [2]), Fragment("part:3", [3])],
            20,
            0.02
        );

        Assert.Single(roots);
        var child = Assert.Single(firstSplit);
        Assert.Equal("part:2", child.FragmentId);
        Assert.Equal("part:0", child.ParentFragmentId);
        Assert.Equal(1, child.Generation);
        var grandchild = Assert.Single(secondSplit);
        Assert.Equal("part:3", grandchild.FragmentId);
        Assert.Equal("part:2", grandchild.ParentFragmentId);
        Assert.Equal(2, grandchild.Generation);
        Assert.Equal(3, tracker.Records.Count);
    }

    [Fact]
    public void UnchangedTopologyDoesNotCreateDuplicateBirths()
    {
        var tracker = new FragmentLineageTracker();
        var fragment = Fragment("part:0", [0, 1]);

        Assert.Single(tracker.Observe([fragment], 0, 0));
        Assert.Empty(tracker.Observe([fragment], 1, 0.001));
        Assert.Single(tracker.Records);
    }

    private static FragmentClusterSnapshot Fragment(string id, int[] nodes) =>
        new(
            id,
            "part",
            nodes,
            nodes.Length,
            Vec3.Zero,
            Vec3.Zero,
            new(Vec3.Zero, Vec3.UnitX),
            false,
            [],
            Matrix3.Identity,
            1,
            Vec3.Zero,
            Vec3.Zero,
            0,
            0,
            0,
            0
        );
}

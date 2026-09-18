using ImpactLab.Extractor;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class CanonicalPathPolicyTests
{
    [Fact]
    public void NormalizesRepositorySeparators() =>
        Assert.Equal("src/a.cs", CanonicalPathPolicy.Normalize("src\\a.cs"));

    [Theory]
    [InlineData("/etc/passwd")]
    [InlineData("../escape.cs")]
    [InlineData("src/../escape.cs")]
    [InlineData("C:\\temp\\escape.cs")]
    [InlineData("\\\\server\\share\\file.cs")]
    public void RejectsRootedOrTraversalPaths(string path) =>
        Assert.Throws<InvalidOperationException>(() => CanonicalPathPolicy.Normalize(path));
}

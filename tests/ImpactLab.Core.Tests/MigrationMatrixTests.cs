using ImpactLab.Core.IO.Migrations;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class MigrationMatrixTests
{
    [Theory]
    [InlineData("scenario")]
    [InlineData("experiment")]
    [InlineData("workspace")]
    public void V15MatrixCoversV3ThroughV5(string type)
    {
        var matrix = MigrationMatrixCatalog.CreateV15();
        Assert.Empty(MigrationCoverageValidator.Validate(matrix, type, 3, 5));
    }
}

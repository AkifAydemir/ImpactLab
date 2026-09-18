using ImpactLab.Core.Continuum.Nonlinear;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class NewmarkPredictorTests
{
    [Fact]
    public void Contract()
    {
        var u = new double[3];
        var v = new[] { 1d, 0, 0 };
        var a = new double[3];
        var up = new double[3];
        var vp = new double[3];
        NewmarkPredictor.Predict(u, v, a, 0.1, 0.25, 0.5, up, vp);
        Assert.Equal(0.1, up[0], 6);
    }
}

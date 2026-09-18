using ImpactLab.Core.Continuum.Meshing;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class VolumeMeshingSettingsTests
{
    [Fact]
    public void Contract()
    {
        new VolumeMeshingSettings().Validate();
    }
}

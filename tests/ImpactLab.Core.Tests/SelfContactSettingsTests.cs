using ImpactLab.Core.Continuum.Contact;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SelfContactSettingsTests
{
    [Fact]
    public void Contract()
    {
        new SurfaceSelfContactSettings().Validate();
    }
}

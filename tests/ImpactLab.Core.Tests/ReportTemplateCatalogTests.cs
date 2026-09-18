using ImpactLab.Core.Reporting.Templates;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ReportTemplateCatalogTests
{
    [Fact]
    public void Contract()
    {
        Assert.NotEmpty(ReportTemplateCatalog.BuiltIns);
    }
}

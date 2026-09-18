using ImpactLab.Core.Results.Expressions;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class FieldExpressionParserTests
{
    [Fact]
    public void ParsesMagnitude() =>
        Assert.IsType<UnaryFieldExpression>(FieldExpressionParser.Parse("mag(displacement)"));
}

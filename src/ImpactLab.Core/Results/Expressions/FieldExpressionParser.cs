namespace ImpactLab.Core.Results.Expressions;

public static class FieldExpressionParser
{
    public static FieldExpression Parse(string text)
    {
        text = text.Trim();
        if (
            double.TryParse(
                text,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var d
            )
        )
            return new ConstantExpression(d);
        if (text.StartsWith("mag(", StringComparison.Ordinal) && text.EndsWith(')'))
            return new UnaryFieldExpression(FieldExpressionKind.Magnitude, Parse(text[4..^1]));
        if (text.StartsWith("vm(", StringComparison.Ordinal) && text.EndsWith(')'))
            return new UnaryFieldExpression(FieldExpressionKind.VonMises, Parse(text[3..^1]));
        foreach (var op in new[] { '+', '-', '*', '/' })
        {
            var i = text.LastIndexOf(op);
            if (i > 0)
            {
                var k = op switch
                {
                    '+' => FieldExpressionKind.Add,
                    '-' => FieldExpressionKind.Subtract,
                    '*' => FieldExpressionKind.Multiply,
                    _ => FieldExpressionKind.Divide,
                };
                return new BinaryFieldExpression(k, Parse(text[..i]), Parse(text[(i + 1)..]));
            }
        }
        return new FieldRefExpression(text);
    }
}

namespace ImpactLab.Core.Results.Expressions;

public static class DerivedFieldEvaluator
{
    public static double[] Evaluate(FieldExpression e, FieldEvaluationContext c) =>
        e switch
        {
            ConstantExpression x => Enumerable.Repeat(x.Value, c.ItemCount).ToArray(),
            FieldRefExpression x => c.ReadField(x.FieldId),
            BinaryFieldExpression x => Binary(x, c),
            UnaryFieldExpression x => Unary(x, c),
            _ => throw new NotSupportedException(),
        };

    static double[] Binary(BinaryFieldExpression e, FieldEvaluationContext c)
    {
        var a = Evaluate(e.Left, c);
        var b = Evaluate(e.Right, c);
        var n = Math.Min(a.Length, b.Length);
        var r = new double[n];
        for (var i = 0; i < n; i++)
            r[i] = e.Operation switch
            {
                FieldExpressionKind.Add => a[i] + b[i],
                FieldExpressionKind.Subtract => a[i] - b[i],
                FieldExpressionKind.Multiply => a[i] * b[i],
                FieldExpressionKind.Divide => a[i]
                    / Math.Max(Math.Abs(b[i]), 1e-30)
                    * Math.Sign(b[i] == 0 ? 1 : b[i]),
                _ => 0,
            };
        return r;
    }

    static double[] Unary(UnaryFieldExpression e, FieldEvaluationContext c)
    {
        var a = Evaluate(e.Operand, c);
        if (e.Operation == FieldExpressionKind.Magnitude)
        {
            var comp = Math.Max(1, c.Components);
            var n = a.Length / comp;
            var r = new double[n];
            for (var i = 0; i < n; i++)
            {
                double s = 0;
                for (var j = 0; j < comp; j++)
                    s += a[i * comp + j] * a[i * comp + j];
                r[i] = Math.Sqrt(s);
            }
            return r;
        }
        return a;
    }
}

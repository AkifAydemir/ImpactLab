namespace ImpactLab.Core.Results.Expressions;

public abstract record FieldExpression(FieldExpressionKind Kind);

public sealed record FieldRefExpression(string FieldId)
    : FieldExpression(FieldExpressionKind.Field);

public sealed record ConstantExpression(double Value)
    : FieldExpression(FieldExpressionKind.Constant);

public sealed record BinaryFieldExpression(
    FieldExpressionKind Operation,
    FieldExpression Left,
    FieldExpression Right
) : FieldExpression(Operation);

public sealed record UnaryFieldExpression(
    FieldExpressionKind Operation,
    FieldExpression Operand,
    int Component = -1
) : FieldExpression(Operation);

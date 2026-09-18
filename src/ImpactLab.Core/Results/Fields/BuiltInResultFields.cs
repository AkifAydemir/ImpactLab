namespace ImpactLab.Core.Results.Fields;

public static class BuiltInResultFields
{
    public static IReadOnlyList<ResultFieldDescriptor> All =>
        [
            new(
                "displacement",
                "Displacement",
                "m",
                ResultFieldAssociation.Node,
                ResultFieldValueType.Vector3,
                3
            ),
            new(
                "velocity",
                "Velocity",
                "m/s",
                ResultFieldAssociation.Node,
                ResultFieldValueType.Vector3,
                3
            ),
            new(
                "stress",
                "Cauchy Stress",
                "Pa",
                ResultFieldAssociation.Element,
                ResultFieldValueType.SymmetricTensor6,
                6
            ),
            new(
                "strain",
                "Strain",
                "1",
                ResultFieldAssociation.Element,
                ResultFieldValueType.SymmetricTensor6,
                6
            ),
            new(
                "eq-plastic-strain",
                "Equivalent Plastic Strain",
                "1",
                ResultFieldAssociation.Element,
                ResultFieldValueType.Scalar,
                1
            ),
            new(
                "temperature",
                "Temperature",
                "K",
                ResultFieldAssociation.Node,
                ResultFieldValueType.Scalar,
                1
            ),
            new(
                "damage",
                "Damage",
                "1",
                ResultFieldAssociation.Element,
                ResultFieldValueType.Scalar,
                1
            ),
        ];
}

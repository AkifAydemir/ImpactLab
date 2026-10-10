namespace ImpactLab.App.Rendering;

public sealed record HeatmapFieldMetadata(
    string DisplayName,
    string QuantityDefinition,
    string Algorithm,
    string InterpretationWarning
)
{
    public string DescribeScale(double maximum, string unit) =>
        $"Linear 0–{maximum:G4} {unit}: dark blue 0%, light blue ≈25%, cyan 50%, yellow 70%, orange 85%, red 100%. Values are clamped at the displayed maximum.";
}

public static class HeatmapFieldCatalog
{
    public static HeatmapFieldMetadata Get(HeatmapMode mode) =>
        mode switch
        {
            HeatmapMode.Damage => new(
                "Spring damage proxy",
                "Dimensionless local average of connected spring damage; 0 is intact and 1 is fully degraded.",
                "ExplicitLatticeSolver spring state projected to boundary nodes.",
                "A lattice damage proxy, not a certified crack length or fracture-surface prediction."
            ),
            HeatmapMode.Displacement => new(
                "Displacement magnitude",
                "Euclidean distance between the current and rest node position.",
                "|x(t) - x(0)| at each lattice node.",
                "Includes elastic motion and rigid/local vibration; a final frame is not automatically permanent deformation."
            ),
            HeatmapMode.Speed => new(
                "Speed magnitude",
                "Euclidean magnitude of nodal velocity.",
                "|v(t)| at each lattice node.",
                "This is instantaneous nodal speed, not impactor speed or wave speed."
            ),
            HeatmapMode.EquivalentStrain => new(
                "Lattice equivalent-strain proxy",
                "Mean absolute axial strain of springs connected to each node.",
                "mean(|(current spring length - rest length) / rest length|).",
                "Not a continuum von Mises/equivalent strain tensor invariant."
            ),
            HeatmapMode.EquivalentStress => new(
                "Lattice equivalent-stress proxy",
                "Mean absolute axial spring stress connected to each node after tangent response and damage reduction.",
                "mean(|sigma_axial|), where sigma follows the configured bilinear lattice material response.",
                "Not continuum von Mises stress and not directly comparable with an FEA von Mises contour."
            ),
            HeatmapMode.YieldUtilization => new(
                "Yield-utilization proxy",
                "Lattice equivalent-stress proxy divided by nominal material yield strength.",
                "sigma_lattice_proxy / nominal yield strength.",
                "A value above 1 flags model utilization; it is not a code-compliance check or factor of safety."
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };
}

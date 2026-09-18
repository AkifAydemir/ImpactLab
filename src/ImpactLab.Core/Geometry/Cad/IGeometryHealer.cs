namespace ImpactLab.Core.Geometry.Cad;

public interface IGeometryHealer
{
    GeometryHealingReport Heal(
        CadModel model,
        GeometryHealingSettings settings,
        CancellationToken ct = default
    );
}

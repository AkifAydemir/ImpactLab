using ImpactLab.Core.Continuum;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Thermal;

public static class ThermalScenarioCompiler
{
    public static ThermalScenarioPackage Compile(ScenarioDefinition s, TetrahedralMesh mesh)
    {
        var table = new ThermalPropertyTable();
        foreach (var m in mesh.Nodes.Select(x => x.Material).DistinctBy(x => x.Id))
        {
            var scale = Math.Clamp(m.DensityKgPerM3 / 7850.0, 0.2, 2.0);
            table.Set(m.Id, new ThermalProperties(45 * scale, 480, 12e-6));
        }
        var bounds = mesh.Bounds;
        IReadOnlyList<int> Select(string? part, double zFrac) =>
            mesh
                .Nodes.Where(n =>
                    (part is null || n.PartId.Equals(part, StringComparison.OrdinalIgnoreCase))
                    && n.Position.Z <= bounds.Min.Z + bounds.Size.Z * zFrac
                )
                .Select(n => n.Id)
                .ToArray();
        var bc = s
            .ThermalBoundaries.Select(x => new ThermalBoundaryCondition(
                Select(x.PartId, x.MaximumNormalizedZ),
                x.TemperatureKelvin
            ))
            .ToArray();
        var sources = s
            .ThermalSources.Select(x => new ThermalSource(
                Select(x.PartId, x.MaximumNormalizedZ),
                x.PowerWatts
            ))
            .ToArray();
        return new(table, bc, sources);
    }
}

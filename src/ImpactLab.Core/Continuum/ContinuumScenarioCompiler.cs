using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Parameters;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Thermal;

namespace ImpactLab.Core.Continuum;

public static class ContinuumScenarioCompiler
{
    public static ContinuumCompiledScenario Compile(
        ScenarioDefinition scenario,
        MaterialCatalog materials
    )
    {
        scenario.Validate();
        var meshes = new List<TetrahedralMesh>();
        var importer = new MeshImportService();
        foreach (
            var part in scenario.Parts.Where(x =>
                x.Behavior != ImpactLab.Core.Scene.ScenePartBehavior.Rigid
            )
        )
        {
            GeometrySpec g =
                part.GeometrySource == ScenarioGeometrySourceKind.ImportedMesh
                    ? new AcceleratedTriangleMeshSpec(
                        part.Name,
                        importer
                            .Import(
                                part.ImportedMeshPath
                                    ?? throw new InvalidOperationException(
                                        $"Part {part.Id} has no imported mesh path."
                                    )
                            )
                            .Asset,
                        ImpactLab.Core.Mathematics.Vec3.Zero
                    )
                    : ParametricGeometryFactory.Create(
                        part.GeometryKind,
                        part.Name,
                        part.GeometryParameters.Clone()
                    );
            if (part.Transform != ImpactLab.Core.Geometry.GeometryTransform3.Identity)
                g = new TransformedGeometrySpec(part.Name, g, part.Transform);
            meshes.Add(
                StructuredTetraMesher.Build(
                    part.Id,
                    g,
                    materials.Get(part.MaterialId),
                    new StructuredTetraMesherSettings(
                        scenario.Continuum.TargetEdgeLengthMeters,
                        scenario.Continuum.MaxCellsPerAxis
                    )
                )
            );
        }
        var merged = TetraMeshAssembler.Combine(meshes);
        return new(merged, scenario, ThermalScenarioCompiler.Compile(scenario, merged));
    }
}

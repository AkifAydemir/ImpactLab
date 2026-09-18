using ImpactLab.Core.Materials;
using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Scene;

public static class SceneMeshCompiler
{
    public static CompiledSceneMesh Compile(
        SimulationScene scene,
        double cellSize,
        MaterialInterfaceTable? interfaceTable = null
    ) => Compile(scene, MeshingSettings.FromCellSize(cellSize), interfaceTable);

    public static CompiledSceneMesh Compile(
        SimulationScene scene,
        MeshingSettings settings,
        MaterialInterfaceTable? interfaceTable = null
    )
    {
        ArgumentNullException.ThrowIfNull(scene);
        settings.Validate();
        var parts = new List<SimulationMesh>();
        var staticIds = new List<string>();
        foreach (var part in scene.Parts)
        {
            if (part.Behavior == ScenePartBehavior.Rigid)
                continue;
            var mesh =
                settings.Mode == MeshingMode.AdaptiveLattice
                    ? AdaptiveLatticeBuilder.Build(
                        part.Id,
                        part.Geometry,
                        part.Material,
                        settings,
                        part.MaterialRegions,
                        interfaceTable
                    )
                    : StructuredMeshBuilder.Build(
                        part.Id,
                        part.Geometry,
                        part.Material,
                        settings.BaseCellSizeMeters,
                        part.MaterialRegions,
                        interfaceTable
                    );
            parts.Add(mesh);
            if (part.Behavior == ScenePartBehavior.Static)
                staticIds.Add(part.Id);
        }
        if (parts.Count == 0)
            throw new InvalidOperationException(
                "Scene contains no deformable or static mesh parts."
            );
        return new CompiledSceneMesh(SimulationMeshAssembler.Combine(parts), staticIds);
    }
}

using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Scene;

public sealed record CompiledSceneMesh(SimulationMesh Mesh, IReadOnlyList<string> StaticPartIds);

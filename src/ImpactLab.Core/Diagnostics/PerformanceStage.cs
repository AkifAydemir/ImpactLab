namespace ImpactLab.Core.Diagnostics;

public enum PerformanceStage
{
    ScenarioCompile,
    MeshBuild,
    SurfaceExtraction,
    InternalForces,
    Loads,
    RigidContact,
    DeformableContact,
    Integration,
    Snapshot,
    PostProcessing,
    Persistence,
    Import,
    Other,
}

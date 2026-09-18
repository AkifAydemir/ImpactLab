using ImpactLab.Core.Continuum.Static;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Results;

public static class ContinuumResultFactory
{
    public static ContinuumResult FromStatic(
        TetrahedralMesh mesh,
        ContinuumStaticResult solved,
        TimeSpan elapsed,
        double[]? temperature = null
    )
    {
        var positions = new Vec3[mesh.Nodes.Count];
        for (var i = 0; i < positions.Length; i++)
            positions[i] = mesh.Nodes[i].Position + solved.Displacement[i];
        var ef = new ContinuumElementFrame(
            solved.ElementStress,
            solved.ElementStrain,
            solved.ElementStress.Select(x => x.VonMises).ToArray()
        );
        var frame = new ContinuumFrame(
            1.0,
            positions,
            solved.Displacement,
            temperature ?? Enumerable.Repeat(293.15, mesh.Nodes.Count).ToArray(),
            ef
        );
        return new(mesh, [frame], [solved.LinearSolve], elapsed);
    }
}

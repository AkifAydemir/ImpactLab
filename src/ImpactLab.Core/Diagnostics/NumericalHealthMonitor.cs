using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Diagnostics;

public static class NumericalHealthMonitor
{
    public static NumericalHealthSample Evaluate(
        int step,
        double time,
        SimulationMesh mesh,
        SimulationState state,
        bool stableEstimate
    )
    {
        double maxV = 0,
            maxA = 0,
            maxD = 0;
        int bad = 0;
        for (var i = 0; i < state.Position.Length; i++)
        {
            var p = state.Position[i];
            var v = state.Velocity[i];
            var f = state.Force[i];
            if (!double.IsFinite(p.X + p.Y + p.Z + v.X + v.Y + v.Z + f.X + f.Y + f.Z))
                bad++;
            maxV = Math.Max(maxV, v.Length);
            maxA = Math.Max(maxA, f.Length / Math.Max(mesh.Nodes[i].MassKg, 1e-12));
            maxD = Math.Max(maxD, (p - mesh.Nodes[i].RestPosition).Length);
        }
        return new(
            step,
            time,
            maxV,
            maxA,
            maxD,
            mesh.Springs.Count == 0 ? 0 : mesh.Springs.Max(x => x.Damage),
            bad,
            stableEstimate
        );
    }
}

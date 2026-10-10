using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;
using ImpactLab.Core.Spatial;

namespace ImpactLab.Core.Contact;

public sealed class DeformableContactSolver : IDeformableContactStepSolver
{
    private readonly SpatialHash3 _hash;
    private readonly List<int> _candidates = [];

    public DeformableContactSolver(double spatialCellSize)
    {
        _hash = new SpatialHash3(spatialCellSize);
    }

    public DeformableContactMetrics Apply(
        SimulationMesh mesh,
        SimulationState state,
        IReadOnlyList<int> surfaceA,
        IReadOnlyList<int> surfaceB,
        string partA,
        string partB,
        ContactSettings settings,
        DeformableContactRule rule,
        double timeStepSeconds = 0.0
    )
    {
        if (!rule.Enabled || surfaceA.Count == 0 || surfaceB.Count == 0)
            return new(partA, partB, 0, 0.0, 0.0);
        _hash.Clear();
        foreach (var id in surfaceB)
            if (state.IsActive[id])
                _hash.Insert(id, state.Position[id]);
        var count = 0;
        var maxOverlap = 0.0;
        var totalNormalForce = 0.0;
        var penaltyEnergy = 0.0;
        var normalDampingEnergy = 0.0;
        var searchRadius = mesh.CellSize * settings.NodeRadiusScale * 2.0;
        var stiffness = settings.NormalStiffnessNPerM * rule.StiffnessScale;
        var damping = settings.NormalDampingNsPerM * rule.DampingScale;
        foreach (var a in surfaceA)
        {
            if (!state.IsActive[a])
                continue;
            var pa = state.Position[a];
            _hash.Query(pa, searchRadius, _candidates);
            foreach (var b in _candidates)
            {
                if (!state.IsActive[b])
                    continue;
                var delta = pa - state.Position[b];
                var distance = delta.Length;
                var target =
                    (mesh.Nodes[a].CellSize + mesh.Nodes[b].CellSize) * settings.NodeRadiusScale;
                var overlap = target - distance;
                if (overlap <= 0.0)
                    continue;
                var normal = distance <= 1e-12 ? Vec3.UnitZ : delta / distance;
                var relativeVelocity = state.Velocity[a] - state.Velocity[b];
                var normalSpeed = Vec3.Dot(relativeVelocity, normal);
                var forceMagnitude = stiffness * overlap - damping * normalSpeed;
                if (forceMagnitude <= 0.0)
                    continue;
                var force = normal * forceMagnitude;
                state.Force[a] += force;
                state.Force[b] -= force;
                count++;
                maxOverlap = Math.Max(maxOverlap, overlap);
                totalNormalForce += forceMagnitude;
                penaltyEnergy += 0.5 * stiffness * overlap * overlap;
                normalDampingEnergy += damping * normalSpeed * normalSpeed * timeStepSeconds;
            }
        }
        return new(
            partA,
            partB,
            count,
            maxOverlap,
            totalNormalForce,
            penaltyEnergy,
            normalDampingEnergy
        );
    }
}

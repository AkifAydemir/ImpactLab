using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;
using ImpactLab.Core.Spatial;

namespace ImpactLab.Core.Contact;

public sealed class DeformableContactSolver
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
        DeformableContactRule rule
    )
    {
        if (!rule.Enabled || surfaceA.Count == 0 || surfaceB.Count == 0)
            return new(partA, partB, 0, 0.0, 0.0);
        _hash.Clear();
        foreach (var id in surfaceB)
            _hash.Insert(id, state.Position[id]);
        var count = 0;
        var maxOverlap = 0.0;
        var totalNormalForce = 0.0;
        var searchRadius = mesh.CellSize * settings.NodeRadiusScale * 2.0;
        var stiffness = settings.NormalStiffnessNPerM * rule.StiffnessScale;
        var damping = settings.NormalDampingNsPerM * rule.DampingScale;
        foreach (var a in surfaceA)
        {
            var pa = state.Position[a];
            _hash.Query(pa, searchRadius, _candidates);
            foreach (var b in _candidates)
            {
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
            }
        }
        return new(partA, partB, count, maxOverlap, totalNormalForce);
    }
}

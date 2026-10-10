using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Fracture;

public static class FragmentClusterAnalyzer
{
    public static IReadOnlyList<FragmentClusterSnapshot> Detached(
        IEnumerable<FragmentClusterSnapshot> clusters
    ) =>
        clusters
            .GroupBy(x => x.PartId, StringComparer.OrdinalIgnoreCase)
            .SelectMany(group =>
            {
                var items = group.OrderByDescending(x => x.MassKg).ToArray();
                if (items.Length <= 1)
                    return [];
                var attached = items.Where(x => x.ContainsConstrainedNode).ToArray();
                return attached.Length > 0
                    ? items.Where(x => !x.ContainsConstrainedNode)
                    : items.Skip(1);
            })
            .ToArray();

    public static IReadOnlyList<FragmentClusterSnapshot> Analyze(
        SimulationMesh mesh,
        SimulationState state
    )
    {
        var parent = Enumerable.Range(0, mesh.Nodes.Count).ToArray();
        foreach (var spring in mesh.Springs)
            if (!spring.IsBroken && state.IsActive[spring.NodeA] && state.IsActive[spring.NodeB])
                Union(parent, spring.NodeA, spring.NodeB);
        return Enumerable
            .Range(0, mesh.Nodes.Count)
            .Where(i => state.IsActive[i])
            .GroupBy(i => (mesh.Nodes[i].PartId, Root(parent, i)))
            .Select(group => Create(group.Key.PartId, group.ToArray(), mesh, state))
            .OrderBy(x => x.PartId, StringComparer.Ordinal)
            .ThenByDescending(x => x.MassKg)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static FragmentClusterSnapshot Create(
        string partId,
        int[] nodeIds,
        SimulationMesh mesh,
        SimulationState state
    )
    {
        var mass = nodeIds.Sum(i => mesh.Nodes[i].MassKg);
        var weightedPosition = Vec3.Zero;
        var momentum = Vec3.Zero;
        var totalNodalKineticEnergy = 0.0;
        foreach (var i in nodeIds)
        {
            weightedPosition += state.Position[i] * mesh.Nodes[i].MassKg;
            momentum += state.Velocity[i] * mesh.Nodes[i].MassKg;
            totalNodalKineticEnergy += 0.5 * mesh.Nodes[i].MassKg * state.Velocity[i].LengthSquared;
        }
        var centerOfMass = weightedPosition / Math.Max(mass, 1e-18);
        var linearVelocity = momentum / Math.Max(mass, 1e-18);
        var i11 = 0.0;
        var i12 = 0.0;
        var i13 = 0.0;
        var i22 = 0.0;
        var i23 = 0.0;
        var i33 = 0.0;
        var angularMomentum = Vec3.Zero;
        foreach (var i in nodeIds)
        {
            var node = mesh.Nodes[i];
            var radius = state.Position[i] - centerOfMass;
            var relativeVelocity = state.Velocity[i] - linearVelocity;
            var pointMass = node.MassKg;
            var cellSelfInertia = pointMass * node.CellSize * node.CellSize / 6.0;
            i11 += pointMass * (radius.Y * radius.Y + radius.Z * radius.Z) + cellSelfInertia;
            i22 += pointMass * (radius.X * radius.X + radius.Z * radius.Z) + cellSelfInertia;
            i33 += pointMass * (radius.X * radius.X + radius.Y * radius.Y) + cellSelfInertia;
            i12 -= pointMass * radius.X * radius.Y;
            i13 -= pointMass * radius.X * radius.Z;
            i23 -= pointMass * radius.Y * radius.Z;
            angularMomentum += Vec3.Cross(radius, relativeVelocity * pointMass);
        }
        var inertia = new Matrix3(i11, i12, i13, i12, i22, i23, i13, i23, i33);
        if (!inertia.TryInverse(out var inverseInertia, out var inertiaConditionNumber))
            throw new InvalidOperationException(
                $"Fragment '{partId}:{nodeIds.Min()}' has a singular or ill-conditioned inertia tensor."
            );
        var angularVelocity = inverseInertia * angularMomentum;
        var translationalKineticEnergy = 0.5 * mass * linearVelocity.LengthSquared;
        var rotationalKineticEnergy = 0.5 * Vec3.Dot(angularMomentum, angularVelocity);
        var unresolvedInternalKineticEnergy = Math.Max(
            0.0,
            totalNodalKineticEnergy - translationalKineticEnergy - rotationalKineticEnergy
        );
        var clusterNodes = nodeIds.ToHashSet();
        var contactSamples = nodeIds
            .Where(i => IsSurfaceNode(mesh.Nodes[i], mesh, clusterNodes))
            .Select(i => new FragmentContactSample(
                state.Position[i] - centerOfMass,
                mesh.Nodes[i].CellSize
            ))
            .ToArray();
        return new(
            $"{partId}:{nodeIds.Min()}",
            partId,
            nodeIds,
            mass,
            centerOfMass,
            linearVelocity,
            BoundingBox3.FromPoints(nodeIds.Select(i => state.Position[i])),
            nodeIds.Any(i => state.IsFixed[i]),
            contactSamples,
            inertia,
            inertiaConditionNumber,
            angularMomentum,
            angularVelocity,
            rotationalKineticEnergy,
            totalNodalKineticEnergy,
            translationalKineticEnergy,
            unresolvedInternalKineticEnergy
        );
    }

    private static bool IsSurfaceNode(
        MeshNode node,
        SimulationMesh mesh,
        IReadOnlySet<int> clusterNodes
    )
    {
        ReadOnlySpan<(int X, int Y, int Z)> directions =
        [
            (1, 0, 0),
            (-1, 0, 0),
            (0, 1, 0),
            (0, -1, 0),
            (0, 0, 1),
            (0, 0, -1),
        ];
        foreach (var direction in directions)
            if (
                !mesh.TryGetNodeId(
                    node.PartId,
                    node.GridX + direction.X,
                    node.GridY + direction.Y,
                    node.GridZ + direction.Z,
                    out var neighborId
                ) || !clusterNodes.Contains(neighborId)
            )
                return true;
        return false;
    }

    private static int Root(int[] parent, int node)
    {
        while (parent[node] != node)
        {
            parent[node] = parent[parent[node]];
            node = parent[node];
        }
        return node;
    }

    private static void Union(int[] parent, int a, int b)
    {
        var rootA = Root(parent, a);
        var rootB = Root(parent, b);
        if (rootA == rootB)
            return;
        if (rootA < rootB)
            parent[rootB] = rootA;
        else
            parent[rootA] = rootB;
    }
}

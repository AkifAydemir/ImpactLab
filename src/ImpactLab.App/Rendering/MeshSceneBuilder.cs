using System.Windows.Media;
using System.Windows.Media.Media3D;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.App.Rendering;

public static class MeshSceneBuilder
{
    private static readonly (int X, int Y, int Z, Vec3 Normal)[] FaceDirections =
    [
        (1, 0, 0, Vec3.UnitX),
        (-1, 0, 0, -Vec3.UnitX),
        (0, 1, 0, Vec3.UnitY),
        (0, -1, 0, -Vec3.UnitY),
        (0, 0, 1, Vec3.UnitZ),
        (0, 0, -1, -Vec3.UnitZ),
    ];

    public static Model3DGroup Build(SimulationMesh mesh, SimulationFrame frame, HeatmapMode mode)
    {
        const int bucketCount = 9;
        var geometries = new MeshGeometry3D[bucketCount];
        for (var i = 0; i < bucketCount; i++)
            geometries[i] = new MeshGeometry3D();
        var maxDisplacement = 1e-12;
        var maxSpeed = 1e-12;
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            maxDisplacement = Math.Max(
                maxDisplacement,
                (frame.Position[i] - mesh.Nodes[i].RestPosition).Length
            );
            maxSpeed = Math.Max(maxSpeed, frame.Velocity[i].Length);
        }
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            var node = mesh.Nodes[i];
            var scalar = mode switch
            {
                HeatmapMode.Damage => frame.NodeDamage[i],
                HeatmapMode.Displacement => (frame.Position[i] - node.RestPosition).Length
                    / maxDisplacement,
                HeatmapMode.Speed => frame.Velocity[i].Length / maxSpeed,
                _ => 0.0,
            };
            var bucket = Math.Clamp((int)Math.Floor(scalar * bucketCount), 0, bucketCount - 1);
            AddBoundaryCube(mesh, frame.Position[i], node, geometries[bucket]);
        }
        var group = new Model3DGroup();
        for (var i = 0; i < bucketCount; i++)
        {
            if (geometries[i].Positions.Count == 0)
                continue;
            var color = GetHeatColor((double)i / (bucketCount - 1));
            var material = new DiffuseMaterial(new SolidColorBrush(color));
            group.Children.Add(
                new GeometryModel3D(geometries[i], material) { BackMaterial = material }
            );
        }
        return group;
    }

    private static void AddBoundaryCube(
        SimulationMesh mesh,
        Vec3 center,
        MeshNode node,
        MeshGeometry3D geometry
    )
    {
        var half = node.CellSize * 0.48;
        foreach (var face in FaceDirections)
        {
            if (
                mesh.TryGetNodeId(
                    node.PartId,
                    node.GridX + face.X,
                    node.GridY + face.Y,
                    node.GridZ + face.Z,
                    out _
                )
            )
            {
                continue;
            }
            AddFace(geometry, center, face.Normal, half);
        }
    }

    private static void AddFace(MeshGeometry3D geometry, Vec3 center, Vec3 normal, double half)
    {
        var reference = Math.Abs(Vec3.Dot(normal, Vec3.UnitZ)) > 0.8 ? Vec3.UnitY : Vec3.UnitZ;
        var u = Vec3.Cross(reference, normal).Normalized() * half;
        var v = Vec3.Cross(normal, u).Normalized() * half;
        var n = normal * half;
        var p0 = center + n - u - v;
        var p1 = center + n + u - v;
        var p2 = center + n + u + v;
        var p3 = center + n - u + v;
        var index = geometry.Positions.Count;
        geometry.Positions.Add(ToPoint3D(p0));
        geometry.Positions.Add(ToPoint3D(p1));
        geometry.Positions.Add(ToPoint3D(p2));
        geometry.Positions.Add(ToPoint3D(p3));
        geometry.TriangleIndices.Add(index + 0);
        geometry.TriangleIndices.Add(index + 1);
        geometry.TriangleIndices.Add(index + 2);
        geometry.TriangleIndices.Add(index + 0);
        geometry.TriangleIndices.Add(index + 2);
        geometry.TriangleIndices.Add(index + 3);
    }

    private static Point3D ToPoint3D(Vec3 value) => new(value.X, value.Y, value.Z);

    private static Color GetHeatColor(double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        if (t < 0.25)
            return Lerp(Colors.Navy, Colors.DodgerBlue, t / 0.25);
        if (t < 0.50)
            return Lerp(Colors.DodgerBlue, Colors.LimeGreen, (t - 0.25) / 0.25);
        if (t < 0.75)
            return Lerp(Colors.LimeGreen, Colors.Gold, (t - 0.50) / 0.25);
        return Lerp(Colors.Gold, Colors.Red, (t - 0.75) / 0.25);
    }

    private static Color Lerp(Color a, Color b, double t) =>
        Color.FromRgb(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t)
        );
}

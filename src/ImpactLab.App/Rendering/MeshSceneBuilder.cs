using System.Windows.Media;
using System.Windows.Media.Media3D;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.App.Rendering;

public static class MeshSceneBuilder
{
    public sealed record HeatmapRenderResult(
        Model3DGroup Scene,
        double Minimum,
        double Maximum,
        double DisplayMaximum,
        string Unit,
        HeatmapFieldMetadata Metadata
    );

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
        return BuildDetailed(mesh, frame, mode).Scene;
    }

    public static HeatmapRenderResult BuildDetailed(
        SimulationMesh mesh,
        SimulationFrame frame,
        HeatmapMode mode,
        double? displayMaximumOverride = null
    )
    {
        const int bucketCount = 16;
        var geometries = new MeshGeometry3D[bucketCount];
        for (var i = 0; i < bucketCount; i++)
            geometries[i] = new MeshGeometry3D();
        var values = EvaluateValues(mesh, frame, mode);
        var activeValues = Enumerable
            .Range(0, values.Length)
            .Where(i => frame.NodeActive is null || frame.NodeActive[i])
            .Select(i => values[i])
            .ToArray();
        var minimum = activeValues.Length == 0 ? 0 : activeValues.Min();
        var maximum = activeValues.Length == 0 ? 0 : activeValues.Max();
        var displayMaximum =
            mode == HeatmapMode.Damage ? 1.0 : Math.Max(displayMaximumOverride ?? maximum, 1e-12);
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            if (frame.NodeActive is not null && !frame.NodeActive[i])
                continue;
            var node = mesh.Nodes[i];
            var scalar = values[i] / displayMaximum;
            var bucket = Math.Clamp(
                (int)Math.Round(scalar * (bucketCount - 1)),
                0,
                bucketCount - 1
            );
            AddBoundaryCube(mesh, frame.Position[i], node, geometries[bucket], frame.NodeActive);
        }
        foreach (var fragment in frame.FragmentCoarseStates ?? [])
        {
            if (frame.NodeActive is null || fragment.SourceNodeIds.All(i => frame.NodeActive[i]))
                continue;
            var averageValue =
                fragment.SourceNodeIds.Count == 0
                    ? 0.0
                    : fragment.SourceNodeIds.Average(i => values[i]);
            var bucket = Math.Clamp(
                (int)Math.Round(averageValue / displayMaximum * (bucketCount - 1)),
                0,
                bucketCount - 1
            );
            var size = fragment.Bounds.Size;
            var half = 0.5 * Math.Max(mesh.CellSize, Math.Max(size.X, Math.Max(size.Y, size.Z)));
            foreach (var face in FaceDirections)
                AddFace(geometries[bucket], fragment.CenterOfMass, face.Normal, half);
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
        return new(
            group,
            minimum,
            maximum,
            displayMaximum,
            Unit(mode),
            HeatmapFieldCatalog.Get(mode)
        );
    }

    public static double MaximumValue(SimulationMesh mesh, SimulationFrame frame, HeatmapMode mode)
    {
        var values = EvaluateValues(mesh, frame, mode);
        var activeValues = Enumerable
            .Range(0, values.Length)
            .Where(i => frame.NodeActive is null || frame.NodeActive[i])
            .Select(i => values[i])
            .ToArray();
        return activeValues.Length == 0 ? 0 : activeValues.Max();
    }

    private static double[] EvaluateValues(
        SimulationMesh mesh,
        SimulationFrame frame,
        HeatmapMode mode
    )
    {
        var derived = mode
            is HeatmapMode.EquivalentStrain
                or HeatmapMode.EquivalentStress
                or HeatmapMode.YieldUtilization
            ? LatticeDerivedFieldCalculator.Evaluate(mesh, frame)
            : null;
        var values = new double[mesh.Nodes.Count];
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            values[i] = mode switch
            {
                HeatmapMode.Damage => frame.NodeDamage[i],
                HeatmapMode.Displacement => (frame.Position[i] - mesh.Nodes[i].RestPosition).Length,
                HeatmapMode.Speed => frame.Velocity[i].Length,
                HeatmapMode.EquivalentStrain => derived!.EquivalentStrain[i],
                HeatmapMode.EquivalentStress => derived!.EquivalentStressPa[i],
                HeatmapMode.YieldUtilization => derived!.EquivalentStressPa[i]
                    / Math.Max(mesh.Nodes[i].Material.EffectiveYieldStrengthPa, 1e-9),
                _ => 0,
            };
        }
        return values;
    }

    private static string Unit(HeatmapMode mode) =>
        mode switch
        {
            HeatmapMode.Displacement => "m",
            HeatmapMode.Speed => "m/s",
            HeatmapMode.EquivalentStress => "Pa",
            _ => "-",
        };

    private static void AddBoundaryCube(
        SimulationMesh mesh,
        Vec3 center,
        MeshNode node,
        MeshGeometry3D geometry,
        bool[]? nodeActive
    )
    {
        var half = node.CellSize * 0.505;
        foreach (var face in FaceDirections)
        {
            if (
                mesh.TryGetNodeId(
                    node.PartId,
                    node.GridX + face.X,
                    node.GridY + face.Y,
                    node.GridZ + face.Z,
                    out var neighborId
                ) && (nodeActive is null || nodeActive[neighborId])
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
        var blue = Color.FromRgb(20, 61, 115);
        var cyan = Color.FromRgb(52, 184, 197);
        var yellow = Color.FromRgb(242, 193, 78);
        var orange = Color.FromRgb(231, 111, 81);
        var red = Color.FromRgb(179, 38, 30);
        if (t < 0.25)
            return Lerp(blue, Colors.DodgerBlue, t / 0.25);
        if (t < 0.50)
            return Lerp(Colors.DodgerBlue, cyan, (t - 0.25) / 0.25);
        if (t < 0.70)
            return Lerp(cyan, yellow, (t - 0.50) / 0.20);
        if (t < 0.85)
            return Lerp(yellow, orange, (t - 0.70) / 0.15);
        return Lerp(orange, red, (t - 0.85) / 0.15);
    }

    private static Color Lerp(Color a, Color b, double t) =>
        Color.FromRgb(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t)
        );
}

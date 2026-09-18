using System.Windows.Media;
using System.Windows.Media.Media3D;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Simulation;

namespace ImpactLab.App.Rendering;

public static class RigidBodySceneBuilder
{
    public static GeometryModel3D Build(RigidBodyDefinition definition, RigidBodyFrame frame)
    {
        var mesh = definition.Geometry switch
        {
            SphereSpec sphere => BuildSphere(sphere.Center, sphere.Radius, 18, 12),
            BoxSpec box => BuildBox(box.Center, box.Size),
            CylinderSpec cylinder => BuildCylinder(
                cylinder.Center,
                cylinder.Radius,
                cylinder.Length,
                24
            ),
            _ => BuildBox(definition.Geometry.Bounds.Center, definition.Geometry.Bounds.Size),
        };
        var material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(205, 210, 218)));
        return new GeometryModel3D(mesh, material)
        {
            BackMaterial = material,
            Transform = BuildTransform(definition.Geometry.Center, frame),
        };
    }

    private static Transform3D BuildTransform(Vec3 pivot, RigidBodyFrame frame)
    {
        var q = frame.Orientation;
        var rotation = new QuaternionRotation3D(new Quaternion(q.X, q.Y, q.Z, q.W));
        var group = new Transform3DGroup();
        group.Children.Add(new TranslateTransform3D(-pivot.X, -pivot.Y, -pivot.Z));
        group.Children.Add(new RotateTransform3D(rotation));
        group.Children.Add(
            new TranslateTransform3D(
                pivot.X + frame.PositionOffset.X,
                pivot.Y + frame.PositionOffset.Y,
                pivot.Z + frame.PositionOffset.Z
            )
        );
        return group;
    }

    private static MeshGeometry3D BuildBox(Vec3 center, Vec3 size)
    {
        var mesh = new MeshGeometry3D();
        var h = size * 0.5;
        var p = new[]
        {
            center + new Vec3(-h.X, -h.Y, -h.Z),
            center + new Vec3(h.X, -h.Y, -h.Z),
            center + new Vec3(h.X, h.Y, -h.Z),
            center + new Vec3(-h.X, h.Y, -h.Z),
            center + new Vec3(-h.X, -h.Y, h.Z),
            center + new Vec3(h.X, -h.Y, h.Z),
            center + new Vec3(h.X, h.Y, h.Z),
            center + new Vec3(-h.X, h.Y, h.Z),
        };
        foreach (var v in p)
            mesh.Positions.Add(ToPoint3D(v));
        AddQuad(mesh, 0, 1, 2, 3);
        AddQuad(mesh, 4, 7, 6, 5);
        AddQuad(mesh, 0, 4, 5, 1);
        AddQuad(mesh, 1, 5, 6, 2);
        AddQuad(mesh, 2, 6, 7, 3);
        AddQuad(mesh, 3, 7, 4, 0);
        return mesh;
    }

    private static MeshGeometry3D BuildSphere(Vec3 center, double radius, int slices, int stacks)
    {
        var mesh = new MeshGeometry3D();
        for (var stack = 0; stack <= stacks; stack++)
        {
            var phi = Math.PI * stack / stacks;
            var z = Math.Cos(phi) * radius;
            var ring = Math.Sin(phi) * radius;
            for (var slice = 0; slice <= slices; slice++)
            {
                var theta = 2.0 * Math.PI * slice / slices;
                mesh.Positions.Add(
                    ToPoint3D(center + new Vec3(Math.Cos(theta) * ring, Math.Sin(theta) * ring, z))
                );
            }
        }
        var row = slices + 1;
        for (var stack = 0; stack < stacks; stack++)
        for (var slice = 0; slice < slices; slice++)
        {
            var a = stack * row + slice;
            var b = a + 1;
            var c = a + row + 1;
            var d = a + row;
            AddQuad(mesh, a, b, c, d);
        }
        return mesh;
    }

    private static MeshGeometry3D BuildCylinder(
        Vec3 center,
        double radius,
        double length,
        int slices
    )
    {
        var mesh = new MeshGeometry3D();
        var half = length * 0.5;
        for (var i = 0; i < slices; i++)
        {
            var angle = 2.0 * Math.PI * i / slices;
            var x = Math.Cos(angle) * radius;
            var y = Math.Sin(angle) * radius;
            mesh.Positions.Add(ToPoint3D(center + new Vec3(x, y, -half)));
            mesh.Positions.Add(ToPoint3D(center + new Vec3(x, y, half)));
        }
        var bottomCenter = mesh.Positions.Count;
        mesh.Positions.Add(ToPoint3D(center + new Vec3(0, 0, -half)));
        var topCenter = mesh.Positions.Count;
        mesh.Positions.Add(ToPoint3D(center + new Vec3(0, 0, half)));
        for (var i = 0; i < slices; i++)
        {
            var next = (i + 1) % slices;
            AddQuad(mesh, i * 2, next * 2, next * 2 + 1, i * 2 + 1);
            mesh.TriangleIndices.Add(bottomCenter);
            mesh.TriangleIndices.Add(next * 2);
            mesh.TriangleIndices.Add(i * 2);
            mesh.TriangleIndices.Add(topCenter);
            mesh.TriangleIndices.Add(i * 2 + 1);
            mesh.TriangleIndices.Add(next * 2 + 1);
        }
        return mesh;
    }

    private static void AddQuad(MeshGeometry3D mesh, int a, int b, int c, int d)
    {
        mesh.TriangleIndices.Add(a);
        mesh.TriangleIndices.Add(b);
        mesh.TriangleIndices.Add(c);
        mesh.TriangleIndices.Add(a);
        mesh.TriangleIndices.Add(c);
        mesh.TriangleIndices.Add(d);
    }

    private static Point3D ToPoint3D(Vec3 value) => new(value.X, value.Y, value.Z);
}

using System.Windows.Media;
using System.Windows.Media.Media3D;
using ImpactLab.Core.PostProcessing;

namespace ImpactLab.App.Rendering;

public static class CutPlaneSceneBuilder
{
    public static Model3DGroup Build(SectionFrame section)
    {
        var group = new Model3DGroup();
        foreach (
            var bucket in section.Nodes.GroupBy(x =>
                Math.Clamp((int)Math.Floor(x.Damage * 8.0), 0, 7)
            )
        )
        {
            var mesh = new MeshGeometry3D();
            foreach (var sample in bucket)
                AddMarker(mesh, sample);
            var t = bucket.Key / 7.0;
            var color = Color.FromRgb(
                (byte)(50 + 205 * t),
                (byte)(160 - 90 * t),
                (byte)(220 - 180 * t)
            );
            var mat = new DiffuseMaterial(new SolidColorBrush(color));
            group.Children.Add(new GeometryModel3D(mesh, mat) { BackMaterial = mat });
        }
        return group;
    }

    private static void AddMarker(MeshGeometry3D mesh, SectionNodeSample s)
    {
        const double h = 0.0015;
        var p = s.Position;
        var i = mesh.Positions.Count;
        mesh.Positions.Add(new Point3D(p.X - h, p.Y - h, p.Z));
        mesh.Positions.Add(new Point3D(p.X + h, p.Y - h, p.Z));
        mesh.Positions.Add(new Point3D(p.X + h, p.Y + h, p.Z));
        mesh.Positions.Add(new Point3D(p.X - h, p.Y + h, p.Z));
        mesh.TriangleIndices.Add(i);
        mesh.TriangleIndices.Add(i + 1);
        mesh.TriangleIndices.Add(i + 2);
        mesh.TriangleIndices.Add(i);
        mesh.TriangleIndices.Add(i + 2);
        mesh.TriangleIndices.Add(i + 3);
    }
}

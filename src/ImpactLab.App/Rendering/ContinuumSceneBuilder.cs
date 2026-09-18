using System.Windows.Media;
using System.Windows.Media.Media3D;
using ImpactLab.App.Visualization;
using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.Results;

namespace ImpactLab.App.Rendering;

public static class ContinuumSceneBuilder
{
    public static Model3DGroup Build(
        ContinuumResult result,
        int frameIndex = 0,
        MeshDecimationSettings? decimation = null
    )
    {
        var f = result.Frames[Math.Clamp(frameIndex, 0, result.Frames.Count - 1)];
        var surface = TetrahedralSurfaceExtractor.Extract(result.Mesh);
        var triangles = decimation is null
            ? surface.Triangles
            : ResultSurfaceDecimator.Decimate(f.Position, surface.Triangles, decimation).Triangles;
        var mesh = new MeshGeometry3D();
        foreach (var t in triangles)
        {
            var baseIndex = mesh.Positions.Count;
            mesh.Positions.Add(To(f.Position[t.A]));
            mesh.Positions.Add(To(f.Position[t.B]));
            mesh.Positions.Add(To(f.Position[t.C]));
            mesh.TriangleIndices.Add(baseIndex);
            mesh.TriangleIndices.Add(baseIndex + 1);
            mesh.TriangleIndices.Add(baseIndex + 2);
        }
        var material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(82, 155, 214)));
        return new Model3DGroup
        {
            Children = { new GeometryModel3D(mesh, material) { BackMaterial = material } },
        };
    }

    private static Point3D To(ImpactLab.Core.Mathematics.Vec3 p) => new(p.X, p.Y, p.Z);
}

using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public static class MeshUnitNormalizer
{
    public static TriangleMeshAsset Normalize(TriangleMeshAsset asset, MeshImportOptions options)
    {
        var scale = options.ScaleToMeters;
        var center = options.CenterAtOrigin ? asset.Bounds.Center : Vec3.Zero;
        var vertices = asset.Vertices.Select(v => (v - center) * scale).ToArray();
        return new TriangleMeshAsset(asset.Name, vertices, asset.Indices.ToArray());
    }
}

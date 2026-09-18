using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public sealed class AcceleratedTriangleMeshSpec : GeometrySpec
{
    private readonly TriangleBvh _bvh;

    public AcceleratedTriangleMeshSpec(string name, TriangleMeshAsset asset, Vec3 translation)
        : base(name, asset.Bounds.Center + translation)
    {
        Asset = asset;
        Translation = translation;
        Bounds = new BoundingBox3(asset.Bounds.Min + translation, asset.Bounds.Max + translation);
        _bvh = new TriangleBvh(asset);
    }

    public TriangleMeshAsset Asset { get; }
    public Vec3 Translation { get; }
    public override GeometryKind Kind => GeometryKind.ImportedMesh;
    public override BoundingBox3 Bounds { get; }

    public override bool Contains(in Vec3 point)
    {
        if (!Bounds.Contains(point))
            return false;
        var local = point - Translation;
        var hits = _bvh.CountRayHits(new Ray3(local, new Vec3(1, 0.173205080756, 0.091287092918)));
        return (hits & 1) == 1;
    }

    public bool Raycast(in Ray3 worldRay, out double distance) =>
        _bvh.Intersects(new Ray3(worldRay.Origin - Translation, worldRay.Direction), out distance);
}

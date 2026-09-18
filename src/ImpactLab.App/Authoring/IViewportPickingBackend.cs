using ImpactLab.Core.Geometry;

namespace ImpactLab.App.Authoring;

public interface IViewportPickingBackend
{
    string Id { get; }
    bool IsAvailable { get; }
    ViewportPickHit? Pick(
        IEnumerable<(string Id, GeometrySpec Geometry)> parts,
        ViewportPickRequest request
    );
}

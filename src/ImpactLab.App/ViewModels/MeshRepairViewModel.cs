using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Geometry.Repair;
using RepairReport = ImpactLab.Core.Geometry.Repair.MeshRepairReport;

namespace ImpactLab.App.ViewModels;

public sealed class MeshRepairViewModel
{
    public MeshRepairOptions Options { get; set; } = new();
    public RepairReport? LastReport { get; private set; }

    public TriangleMeshAsset Repair(TriangleMeshAsset asset)
    {
        var r = new MeshRepairPipeline().Repair(asset, Options);
        LastReport = r.Report;
        return r.Asset;
    }
}

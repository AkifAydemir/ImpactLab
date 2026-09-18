using System.Windows.Media.Media3D;
using ImpactLab.Core.Backends;
using ImpactLab.Core.Continuum.Results;

namespace ImpactLab.App.Rendering;

public static class ResultDomainSceneBuilder
{
    public static Model3DGroup Build(BackendRunOutput output, int frameIndex, HeatmapMode heatmap)
    {
        return output.Domain switch
        {
            LatticeResultDomain lattice => MeshSceneBuilder.Build(
                lattice.Mesh,
                output.Result.Frames[Math.Clamp(frameIndex, 0, output.Result.Frames.Count - 1)],
                heatmap
            ),
            ImpactLab.Core.Continuum.ContinuumResultDomain => ContinuumSceneBuilder.Build(
                output.Native<ContinuumResult>()
                    ?? throw new InvalidOperationException("Continuum native result missing."),
                frameIndex
            ),
            _ => new Model3DGroup(),
        };
    }
}
